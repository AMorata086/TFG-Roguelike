using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SoundEffectReferences : ScriptableObject
{
    public AudioClip PlayerShot;
    public AudioClip EnemyRangedShot;
    public AudioClip EnemyDroneShot;
    public AudioClip PlayerHurt;
    public AudioClip EnemyHurt;
    public AudioClip EnemyDies;
}
