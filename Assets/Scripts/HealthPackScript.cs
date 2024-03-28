using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPackScript: MonoBehaviour
{
    [SerializeField] private int totalHealth;
    [SerializeField] private int healthPerTick;
    private DamageEffect damageVfx;

    private void Awake()
    {
        damageVfx = gameObject.GetComponent<DamageEffect>();
    }

    public int GetHealth()
    {
        totalHealth -= healthPerTick;
        damageVfx.CallDamageEffect();
        if (totalHealth <= 0)
        {
            Destroy(gameObject);
        }
        return healthPerTick;
    }
}
