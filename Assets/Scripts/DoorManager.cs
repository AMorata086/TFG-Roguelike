using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorManager : MonoBehaviour
{
    // Variables
    public Tilemap doorsClosed;
    public Tilemap doorsOpen;

    public void SwitchDoorState ()
    {
        if (!doorsOpen.gameObject.activeSelf)
        {
            doorsClosed.gameObject.SetActive(false);
            doorsOpen.gameObject.SetActive(true);
            Debug.Log("Doors state changed to open.");
        } 
        else if(doorsOpen.gameObject.activeSelf)
        {
            doorsClosed.gameObject.SetActive(true);
            doorsOpen.gameObject.SetActive(false);
            Debug.Log("Doors state changed to closed.");
        } 
    }
}
