using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
public class Amb_Zone_Inner : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Gizmos.color = new Color(Color.red.r,
            Color.red.g,
            Color.red.b,
            0.3f); ;
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
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Amb_EventTrigger ambEventTrigger))
        {
            //turn off the outer bound
            gameObject.transform.parent.GetComponent<Amb_ZoneOuter>().priority = 1f;
            gameObject.transform.parent.GetComponent<BoxCollider>().enabled = false;
            ambEventTrigger.SetAmbDistanceAmount(gameObject.transform.parent.GetComponent<Amb_ZoneOuter>().type, 0f,
                gameObject.transform.parent.gameObject);
            
        }        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Amb_EventTrigger ambEventTrigger))
        {
            //turn off the outer bound
            gameObject.transform.parent.GetComponent<Amb_ZoneOuter>().priority = 1f;
            gameObject.transform.parent.GetComponent<BoxCollider>().enabled = true;
            ambEventTrigger.SetAmbDistanceAmount(gameObject.transform.parent.GetComponent<Amb_ZoneOuter>().type, 0f,
            gameObject.transform.parent.gameObject);
            
        }
    }
}
