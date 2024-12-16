using UnityEngine;
using static Amb_EventTrigger;
[RequireComponent(typeof(BoxCollider))]

public class Amb_ZoneOuter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public AmbZoneDefinition type;
    //distance from the edge of the area where we lerp the parameter value from 0 to 1
    [SerializeField, Range(0f, 1f)]
    public float falloffDistanceX = 0.9f;
    [SerializeField, Range(0f, 1f)]
    public float falloffDistanceY = 0.9f;
    [SerializeField, Range(0f, 1f)]
    public float falloffDistanceZ = 0.9f;
    private float realDistanceFallOffX = 0f;
    private float realDistanceFallOffY = 0f;
    private float realDistanceFallOffZ = 0f;
    private Vector3 distance = new(0f,0f,0f);
    public float priority = 0f;

    private Vector3 closestPoint = new();
    private BoxCollider m_BoxCollider;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(Color.yellow.r,
            Color.yellow.g,
            Color.yellow.b,
            0.2f);
        // Get the BoxCollider component
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        // Calculate the center and size in world space
        Vector3 center = transform.TransformPoint(boxCollider.center);
        Vector3 size = Vector3.Scale(boxCollider.size, transform.lossyScale);

        // Use the GameObject's rotation
        Quaternion rotation = transform.rotation;

        // Save the current Gizmos matrix
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // Create a transformation matrix
        Matrix4x4 matrix = Matrix4x4.TRS(center, rotation, size);

        // Set the Gizmos matrix to the transformation matrix
        Gizmos.matrix = matrix;     

        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        // Restore the original Gizmos matrix
        Gizmos.matrix = oldMatrix;
    }

    private void OnValidate()
    {
        m_BoxCollider = gameObject.transform.GetChild(0).GetComponent<BoxCollider>();
        m_BoxCollider.isTrigger = true;
        m_BoxCollider.size = new Vector3(GetComponent<BoxCollider>().size.x * falloffDistanceX,
            GetComponent<BoxCollider>().size.y * falloffDistanceY,
            GetComponent<BoxCollider>().size.z * falloffDistanceZ);
        m_BoxCollider.center = GetComponent<BoxCollider>().center;
    }

    private void Start()
    {
        m_BoxCollider = gameObject.transform.GetChild(0).GetComponent<BoxCollider>();
        //grab 1 axis and multiple by the falloff distance to get the distance between the inner and outer bounds
        realDistanceFallOffX = GetComponent<BoxCollider>().size.x * (1f - falloffDistanceX) * transform.lossyScale.x;
        realDistanceFallOffY = GetComponent<BoxCollider>().size.y * (1f - falloffDistanceY) * transform.lossyScale.y;
        realDistanceFallOffZ = GetComponent<BoxCollider>().size.z * (1f - falloffDistanceZ) * transform.lossyScale.z;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Amb_EventTrigger ambEventTrigger))
        {
            priority = 0;
            ambEventTrigger.SetAmbDistanceAmount(type,1,gameObject);
        }    
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Amb_EventTrigger ambEventTrigger))
        {
            closestPoint = m_BoxCollider.ClosestPointOnBounds(other.gameObject.transform.position);
            distance = (closestPoint - other.gameObject.transform.position);
            //If the value of any axis is 0 that means we are either on the innner bound, in which case this shouldn't run anyway or
            // we are actually inside one of the edges of the other sides of inner to outer bound
            // on corners it will fade out using a priority of x, z and then y. Mainly because typically the character will be non 0 in either
            // the x or z axis before z axis 
            //we are also using this to drive the priority value given top this zone in case of a collision between two zone which are of the same type
            //the highest set value is prioritized and is sent to FMOD.
            if (Mathf.Abs(distance.x) < realDistanceFallOffX && Mathf.Abs(distance.x) > 0)
            {
                priority = 1f - Normalize(Mathf.Abs(distance.x), 0f, realDistanceFallOffX);
                ambEventTrigger.SetAmbDistanceAmount(type, Normalize(Mathf.Abs(distance.x), 0f, realDistanceFallOffX),gameObject);
                
            }
            else if (Mathf.Abs(distance.z) < realDistanceFallOffZ && Mathf.Abs(distance.z) > 0)
            {
                priority = 1f - Normalize(Mathf.Abs(distance.z), 0f, realDistanceFallOffZ);
                ambEventTrigger.SetAmbDistanceAmount(type, Normalize(Mathf.Abs(distance.z), 0f, realDistanceFallOffZ), gameObject);
                
            }
            else if (Mathf.Abs(distance.y) < realDistanceFallOffY && Mathf.Abs(distance.y) > 0)
            {
                priority = 1f - Normalize(Mathf.Abs(distance.y), 0f, realDistanceFallOffY);
                ambEventTrigger.SetAmbDistanceAmount(type, Normalize(Mathf.Abs(distance.y), 0f, realDistanceFallOffY), gameObject);
                
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Amb_EventTrigger ambEventTrigger))
        {
            priority = 0;
            ambEventTrigger.SetAmbDistanceAmount(type, 1, gameObject);
            //if we leave the area, we should clear this from the history as it's not relevant anymore
            if (ambEventTrigger.ambZoneHistory[type].Equals(gameObject))
            {
                ambEventTrigger.ambZoneHistory.Remove(type);
            }
        }

    }

    public static float Normalize(float value, float min, float max) { return (value - min) / (max - min); }
}
