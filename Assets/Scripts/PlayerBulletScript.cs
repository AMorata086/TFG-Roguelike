using UnityEngine;

public class PlayerBulletScript : MonoBehaviour
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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Enemy_Melee":
                collision.gameObject.GetComponentInParent<EnemyMeleeController>().GetHurt(Damage);
                collision.gameObject.GetComponentInParent<Rigidbody2D>().AddForce(gameObject.GetComponent<Rigidbody2D>().velocity.normalized * forceOfImpact * Time.fixedDeltaTime, ForceMode2D.Impulse);
                TriggerParticleEffects();
                Destroy(gameObject);
                break;
            case "Enemy_Ranged":
                collision.gameObject.GetComponentInParent<EnemyRangeController>().GetHurt(Damage);
                collision.gameObject.GetComponentInParent<Rigidbody2D>().AddForce(gameObject.GetComponent<Rigidbody2D>().velocity.normalized * forceOfImpact * Time.fixedDeltaTime, ForceMode2D.Impulse);
                TriggerParticleEffects();
                Destroy(gameObject);
                break;
            case "Enemy_Flying":
                collision.gameObject.GetComponentInParent<EnemyFlyingController>().GetHurt(Damage);
                collision.gameObject.GetComponentInParent<Rigidbody2D>().AddForce(gameObject.GetComponent<Rigidbody2D>().velocity.normalized * forceOfImpact * Time.fixedDeltaTime, ForceMode2D.Impulse);
                TriggerParticleEffects();
                Destroy(gameObject);
                break;
            case "Environment":
                TriggerParticleEffects();
                Destroy(gameObject);
                break;
            case "Player":
                // No PvP implemented
                break;
            case "Enemy_Projectile":
                TriggerParticleEffects();
                Destroy(gameObject);
                break;
            default:
                break;
        }
    }
}
