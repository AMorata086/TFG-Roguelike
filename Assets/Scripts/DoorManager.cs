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
            SoundEffectManager.Instance.PlaySound(SoundEffectManager.Instance.SFXRefs.DoorsOpening, gameObject.transform.position);
        } 
        else if(doorsOpen.gameObject.activeSelf)
        {
            doorsClosed.gameObject.SetActive(true);
            doorsOpen.gameObject.SetActive(false);
            SoundEffectManager.Instance.PlaySound(SoundEffectManager.Instance.SFXRefs.DoorsClosing, gameObject.transform.position);
        } 
    }
}
