using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public SoundEffectReferences SFXRefs;

    public void PlaySound(AudioClip soundEffect, Vector3 position, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(soundEffect, position, volume);
    }
}
