using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEffect : MonoBehaviour
{
    [SerializeField] private Material originalSpriteMaterial;
    [SerializeField] private Material spawnEffectMaterial;

    private SpriteRenderer[] spriteRenderers;
    private float effectQuantity = 0f;
    [SerializeField] private float incrementQuantity = 0.01f;

    private Coroutine spawnEffectCoroutine;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private IEnumerator Spawn()
    {
        // Change the sprite material to the spawn effect material
        ChangeMaterial(spawnEffectMaterial);
        // Increment the effect quantity periodically
        while (effectQuantity <= 1f)
        {
            ChangeQuantityInMaterial(effectQuantity);
            effectQuantity += incrementQuantity;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        // Change back the material to the original one when the spawn effect has ended
        ChangeMaterial(originalSpriteMaterial);
        yield return null;
    }

    public void CallSpawnEffect()
    {
        spawnEffectCoroutine = StartCoroutine(Spawn());
    }

    private void ChangeMaterial(Material material)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].material = material;
        }
    }

    private void ChangeQuantityInMaterial(float effectQuantity)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].material.SetFloat("_EffectQuantity", effectQuantity);
        }
    }

    public float GetSpawnEffectDuration()
    {
        return (1f / incrementQuantity) * Time.fixedDeltaTime;
    }
}
