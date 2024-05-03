using Unity.Netcode;
using UnityEngine;

public class EnemyBulletScript : NetworkBehaviour
{
    public float forceOfImpact = 500f;
    public int Damage = 0;
    public ParticleSystem ImpactParticlesPrefab;

    void TriggerParticleEffects()
    {
        Quaternion qAngle = gameObject.transform.rotation;
        Vector3 eAngles = qAngle.eulerAngles;
        eAngles = new Vector3(-eAngles.z, 90, 0);
        ParticleSystem ImpactParticles = ParticleSystem.Instantiate(ImpactParticlesPrefab, gameObject.transform.position, Quaternion.Euler(eAngles));
        ImpactParticles.transform.localScale = new Vector3(1, 1, -1);
        // Destroy(ImpactParticles.gameObject, ImpactParticles.main.duration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!IsServer)
        {
            return;
        }

        switch (collision.gameObject.tag)
        {
            case "Environment":
                InstantiateParticleEffectsClientRpc();
                Destroy(gameObject);
                break;
            case "Player_Hitbox":
                collision.gameObject.GetComponentInParent<PlayerController>().GetHurt(Damage);
                NetworkObject playerHitNetworkObject = collision.gameObject.GetComponentInParent<NetworkObject>();
                Vector2 knockbackForceApplied = gameObject.GetComponent<Rigidbody2D>().velocity.normalized * forceOfImpact * Time.fixedDeltaTime;
                AddKnockbackForceClientRpc(playerHitNetworkObject, knockbackForceApplied);
                InstantiateParticleEffectsClientRpc();
                Destroy(gameObject);
                break;
            case "Player_1_Bullet":
                InstantiateParticleEffectsClientRpc();
                Destroy(gameObject);
                break;
            case "Player_2_Bullet":
                InstantiateParticleEffectsClientRpc();
                Destroy(gameObject);
                break;
            default:
                break;
        }
    }

    [ClientRpc]
    private void AddKnockbackForceClientRpc(NetworkObjectReference playerHitNetworkObjectReference, Vector2 knockbackForceAppliedToTarget)
    {
        if (playerHitNetworkObjectReference.TryGet(out NetworkObject targetPlayerNetworkObject))
        {
            if (!targetPlayerNetworkObject.IsOwner)
            {
                return;
            }
            Rigidbody2D targetPlayerRigidbody = targetPlayerNetworkObject.gameObject.GetComponentInParent<Rigidbody2D>();
            Debug.Log("Force applied to target: " + knockbackForceAppliedToTarget);
            targetPlayerRigidbody.AddForce(knockbackForceAppliedToTarget, ForceMode2D.Impulse);
        }
        else
        {
            Debug.Log("Error: Couldn't find target player Network Object");
        }
    }

    [ClientRpc]
    private void InstantiateParticleEffectsClientRpc()
    {
        TriggerParticleEffects();
    }
}
