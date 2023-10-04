using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorManager : MonoBehaviour
{
    // Variables
    public Tilemap doorsClosed;
    public Tilemap doorsOpen;

    void SwitchDoorState ()
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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
