using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HealthPackScript: NetworkBehaviour
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
        if(!IsHost)
        {
            return 0;
        }
        totalHealth -= healthPerTick;
        InvokeDamageVfxClientRpc();
        if (totalHealth <= 0)
        {
            Destroy(gameObject);
        }
        return healthPerTick;
    }

    [ClientRpc]
    private void InvokeDamageVfxClientRpc()
    {
        damageVfx.CallDamageEffect();
    }
}
