using System.Data.Common;
using Unity.Netcode;
using UnityEngine;

public class PlayerBulletScript : NetworkBehaviour
{
    public float forceOfImpact = 500f;
    public int Damage = 0;
    public ParticleSystem ImpactParticlesPrefab;
    public string BulletOwnerTag;
    [SerializeField] private PlayerSpritesReferences playerSpritesReferences;
    private SpriteRenderer bulletSpriteRenderer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        bulletSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public void ChangeBulletSpriteAndMaterial(string bulletOwnerTag)
    {
        ChangeSpriteRendererClientRpc(bulletOwnerTag);
    }

    [ClientRpc]
    private void ChangeSpriteRendererClientRpc(string bulletOwnerTag)
    {
        BulletOwnerTag = bulletOwnerTag;
        switch (BulletOwnerTag)
        {
            case "Player_1_Bullet":
                Debug.Log("Pasa por el Player 1");
                bulletSpriteRenderer.sprite = playerSpritesReferences.Player1BulletSprite;
                bulletSpriteRenderer.material = playerSpritesReferences.Player1BulletMaterial;
                break;
            case "Player_2_Bullet":
                Debug.Log("Pasa por el Player 2");
                bulletSpriteRenderer.sprite = playerSpritesReferences.Player2BulletSprite;
                bulletSpriteRenderer.material = playerSpritesReferences.Player2BulletMaterial;
                break;
        }
    }

    [ClientRpc]
    void TriggerParticleEffectsClientRpc()
    {
        Quaternion qAngle = gameObject.transform.rotation;
        Vector3 eAngles = qAngle.eulerAngles;
        eAngles = new Vector3(-eAngles.z, 90, 0);
        ParticleSystem ImpactParticles = ParticleSystem.Instantiate(ImpactParticlesPrefab, gameObject.transform.position, Quaternion.Euler(eAngles));
        ImpactParticles.transform.localScale = new Vector3(1, 1, -1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsHost)
        {
            return;
        }

        switch (collision.gameObject.tag)
        {
            case "Enemy_Melee":
                collision.gameObject.GetComponentInParent<EnemyMeleeController>().GetHurt(Damage);
                collision.gameObject.GetComponentInParent<Rigidbody2D>().AddForce(gameObject.GetComponent<Rigidbody2D>().velocity.normalized * forceOfImpact * Time.fixedDeltaTime, ForceMode2D.Impulse);
                TriggerParticleEffectsClientRpc();
                Destroy(gameObject);
                break;
            case "Enemy_Ranged":
                collision.gameObject.GetComponentInParent<EnemyRangeController>().GetHurt(Damage);
                collision.gameObject.GetComponentInParent<Rigidbody2D>().AddForce(gameObject.GetComponent<Rigidbody2D>().velocity.normalized * forceOfImpact * Time.fixedDeltaTime, ForceMode2D.Impulse);
                TriggerParticleEffectsClientRpc();
                Destroy(gameObject);
                break;
            case "Enemy_Flying":
                collision.gameObject.GetComponentInParent<EnemyFlyingController>().GetHurt(Damage);
                collision.gameObject.GetComponentInParent<Rigidbody2D>().AddForce(gameObject.GetComponent<Rigidbody2D>().velocity.normalized * forceOfImpact * Time.fixedDeltaTime, ForceMode2D.Impulse);
                TriggerParticleEffectsClientRpc();
                Destroy(gameObject);
                break;
            case "Environment":
                TriggerParticleEffectsClientRpc();
                Destroy(gameObject);
                break;
            case "Player":
                // No PvP implemented
                break;
            case "Enemy_Projectile":
                TriggerParticleEffectsClientRpc();
                Destroy(gameObject);
                break;
            case "Health_Pack":
                TriggerParticleEffectsClientRpc();
                //Player.Heal(collision.gameObject.GetComponent<HealthPackScript>().GetHealth());
                Destroy(gameObject);
                break;
            default:
                break;
        }
    }
}
