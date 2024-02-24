using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DamageEffect : MonoBehaviour
{
    [SerializeField] private Material spriteLitMaterial;
    [SerializeField] private Material damageEffectMaterial;
    [SerializeField] private float effectTime = 0.1f;

    private SpriteRenderer[] spriteRenderers;

    private Coroutine damageEffectCoroutine;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void CallDamageEffect()
    {
        damageEffectCoroutine = StartCoroutine(DamageEffectInvoker());
    }

    private IEnumerator DamageEffectInvoker()
    {
        // change the material to the damage effect material
        ChangeMaterial(damageEffectMaterial);
        // wait for the specified amount
        yield return new WaitForSeconds(effectTime);
        // change the material to the original
        ChangeMaterial(spriteLitMaterial);
        yield return null;
    }

    private void ChangeMaterial(Material material)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].material = material;
        }
    }
}
