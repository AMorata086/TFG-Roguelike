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
        switch (collision.gameObject.tag)
        {
            case "Environment":
                TriggerParticleEffects();
                Destroy(gameObject);
                break;
            case "Player":
                collision.gameObject.GetComponentInParent<PlayerController>().GetHurt(Damage);
                collision.gameObject.GetComponentInParent<Rigidbody2D>().AddForce(forceOfImpact * Time.fixedDeltaTime * gameObject.GetComponent<Rigidbody2D>().velocity.normalized, ForceMode2D.Impulse);
                TriggerParticleEffects();
                Destroy(gameObject);
                break;
            case "Player_Projectile":
                TriggerParticleEffects();
                Destroy(gameObject);
                break;
            default:
                break;
        }
    }
}
