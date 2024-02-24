using UnityEngine;

public class ParticleSystemRemover : MonoBehaviour
{
    private ParticleSystem AssociatedVFX;

    void Start()
    {
        AssociatedVFX = GetComponent<ParticleSystem>();
        Destroy(gameObject, AssociatedVFX.main.duration);
    }
}
