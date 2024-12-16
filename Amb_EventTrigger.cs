using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Amb_EventTrigger : MonoBehaviour
{
    public EventReference amb_Event;
    EventInstance amb_Instance;

    public enum AmbZoneDefinition
    {
        Amb_Concourse,
        Amb_Hallway,
        Amb_Office,
        Amb_Hangar
    }

    public Dictionary<AmbZoneDefinition, GameObject> ambZoneHistory = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!amb_Event.IsNull)
        {
            amb_Instance = RuntimeManager.CreateInstance(amb_Event);
            amb_Instance.start();
        }
    }

    private void OnDestroy()
    {
        if(amb_Instance.isValid())
        {
            amb_Instance.release();
            amb_Instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    public void SetAmbDistanceAmount(AmbZoneDefinition ambZoneType, float value, GameObject incomingAmbZone)
    {
        //need to handle the instance where multiple areas overlap and conflict on the values they're sending to the rtpc
        //have we already got an object of this type in our history?
     if(ambZoneHistory.ContainsKey(ambZoneType))
        {
            //is the object the same object as we already have? in which case we can just set the value
            if(incomingAmbZone.Equals(ambZoneHistory[ambZoneType]))
            {
                if (amb_Instance.isValid())
                {
                 amb_Instance.setParameterByName(ambZoneType.ToString(), value);
                }
            }
            //this is a different amb zone, now we need to check their prioritiy values and which one is higher
            //higher priority is given to the zone that our fade out value is the highest in, i.e the louder one
            else
            {
                if (incomingAmbZone.GetComponent<Amb_ZoneOuter>().priority > ambZoneHistory[ambZoneType].GetComponent<Amb_ZoneOuter>().priority)
                {
                    ambZoneHistory[ambZoneType] = incomingAmbZone;
                    amb_Instance.setParameterByName(ambZoneType.ToString(), value);
                }
            }
        }
        //this is the first instance of the type of zone and we can add it to our history and then set the value
        else
        {
            ambZoneHistory.Add(ambZoneType, incomingAmbZone);
            if (amb_Instance.isValid())
            {
                amb_Instance.setParameterByName(ambZoneType.ToString(), value);
            }
        }

    }
}
