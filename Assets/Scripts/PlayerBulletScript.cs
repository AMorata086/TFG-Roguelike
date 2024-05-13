using System.Data.Common;
using Unity.Netcode;
using UnityEngine;

public class PlayerBulletScript : NetworkBehaviour
{
    public float forceOfImpact = 500f;
    public int Damage = 0;
    public ParticleSystem ImpactParticlesPrefab;
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
        gameObject.tag = bulletOwnerTag;
        switch (gameObject.tag)
        {
            case "Player_1_Bullet":
                bulletSpriteRenderer.sprite = playerSpritesReferences.Player1BulletSprite;
                bulletSpriteRenderer.material = playerSpritesReferences.Player1BulletMaterial;
                break;
            case "Player_2_Bullet":
                bulletSpriteRenderer.sprite = playerSpritesReferences.Player2BulletSprite;
                bulletSpriteRenderer.material = playerSpritesReferences.Player2BulletMaterial;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
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
                InstantiateRicochetSfxClientRpc();
                Destroy(gameObject);
                break;
            case "Player":
                // No PvP implemented
                break;
            case "Enemy_Projectile":
                TriggerParticleEffectsClientRpc();
                InstantiateRicochetSfxClientRpc();
                Destroy(gameObject);
                break;
            case "Health_Pack":
                TriggerParticleEffectsClientRpc();
                InstantiateRicochetSfxClientRpc();
                HealthPackScript healthPackScript = collision.gameObject.GetComponent<HealthPackScript>();
                if(healthPackScript == null)
                {
                    Debug.LogError("Error: couldn't find HealthPackScript component in the collided GameObject");
                    return;
                }
                string playerTag = "";
                switch(gameObject.tag)
                {
                    case "Player_1_Bullet":
                        playerTag = "Player_1";
                        break;
                    case "Player_2_Bullet":
                        playerTag = "Player_2";
                        break;
                }
                GameObject playerGameObject = GameObject.FindGameObjectWithTag(playerTag);
                if(playerGameObject == null)
                {
                    Debug.LogError("Error: couldn't find desired player GameObject");
                    return;
                }
                PlayerController playerGameObjectPlayerController = playerGameObject.GetComponent<PlayerController>();
                if(playerGameObjectPlayerController == null)
                {
                    Debug.LogError("Error: couldn't find PlayerController component in Player GameObject");
                    return;
                }
                playerGameObjectPlayerController.Heal(healthPackScript.GetHealth());

                Destroy(gameObject);
                break;
            default:
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

    [ClientRpc]
    private void InstantiateRicochetSfxClientRpc()
    {
        SoundEffectManager.Instance.PlaySound(SoundEffectManager.Instance.SFXRefs.BulletRicochet, gameObject.transform.position);
    }
}
