using Pathfinding;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemyMeleeController : NetworkBehaviour
{
    /* VARIABLES */
    // Enemy Stats
    [Header("Enemy Stats")]
    public int HealthPoints = 10;
    [SerializeField] private int damage = 5;
    [SerializeField] private float knockbackForce = 2000f;
    public float MovementSpeed = 2500f;
    public Rigidbody2D Rb;

    public Animator Animator;
    [SerializeField] private ParticleSystem deathVFX;
    [SerializeField] private ParticleSystem explosionVFX;
    [SerializeField] private SpawnEffect spawnEffect;
    private DamageEffect damageVFX;

    private GameManager gameManager;

    // AI Pathfinding variables
    [Header("AI Pathfinding Variables")]
    Transform target;
    [SerializeField] private float NextWaypointDistance = 1f;
    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    Seeker seeker;

    Vector2 direction;
    float distanceToTarget;
    bool isChasingTarget = false;
    public float TargetDetectionDistance = 10f;

    public void GetHurt(int damageReceived)
    {
        HealthPoints -= damageReceived;
        InstantiateDamageVfxClientRpc();
    }

    [ClientRpc]
    private void InstantiateDamageVfxClientRpc()
    {
        damageVFX.CallDamageEffect();
    }

    private IEnumerator PerformSpawn()
    {
        spawnEffect.CallSpawnEffect();
        yield return new WaitForSeconds(1);
        if(IsServer)
        {
            InvokeRepeating(nameof(UpdatePath), 0f, 0.1f);

        }
        yield return null;
    }

    private void PerformDeath()
    {
        if (HealthPoints <= 0)
        {
            gameManager.DecrementCurrentEnemies();
            InstantiateDeathVfxClientRpc();
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    private void InstantiateDeathVfxClientRpc()
    {
        ParticleSystem.Instantiate(deathVFX, gameObject.transform.position, gameObject.transform.rotation);
        ParticleSystem.Instantiate(explosionVFX, gameObject.transform.position, gameObject.transform.rotation);
    }

    private void AttackTarget(Collider2D collision)
    {
        // invoke the GetHurt method from the player's controller
        collision.gameObject.GetComponentInParent<PlayerController>().GetHurt(damage);
        // Apply a knockback to the player when in contact with the enemy
        NetworkObject playerHitNetworkObject = collision.gameObject.GetComponentInParent<NetworkObject>();

        Vector2 knockbackForceApplied = gameObject.GetComponent<Rigidbody2D>().velocity.normalized * knockbackForce * Time.fixedDeltaTime;

        AddKnockbackForceClientRpc(playerHitNetworkObject, knockbackForceApplied);
    }

    [ClientRpc]
    private void AddKnockbackForceClientRpc(NetworkObjectReference playerHitNetworkObjectReference, Vector2 knockbackForceAppliedToTarget)
    {
        if(playerHitNetworkObjectReference.TryGet(out NetworkObject targetPlayerNetworkObject))
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

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (!IsServer)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player_Hitbox"))
        {
            // Attack the player
            AttackTarget(collision);
        }
    }

    private Transform FindTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player_Hitbox");
        Transform finalTarget;

        if (targets == null)
        {   // Control if there are any errors when searching for a target
            return null;
        }

        // Pursue the nearest target
        if (targets.Length > 1)
        {
            float distanceToTarget1 = Mathf.Abs(Vector2.Distance(targets[0].GetComponentInParent<Rigidbody2D>().position, Rb.position));
            float distanceToTarget2 = Mathf.Abs(Vector2.Distance(targets[1].GetComponentInParent<Rigidbody2D>().position, Rb.position));

            if (distanceToTarget1 < distanceToTarget2)
            {
                 finalTarget = targets[0].transform;
            }
            else
            {
                finalTarget = targets[1].transform;
            }
        }
        else
        {
            finalTarget = targets[0].transform;
        }

        Debug.Log("Target is --> " + finalTarget.parent.tag);
        return finalTarget;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 1;
        }
    }

    private void AnimateEnemy()
    {
        if (direction.sqrMagnitude == 0)
        {
            Animator.SetBool("Moving", false);
        }
        else
        {
            Animator.SetBool("Moving", true);
            Animator.SetFloat("Moving_direction", direction.normalized.x);
        }
    }

    private void UpdatePath()
    {
        if (seeker.IsDone() && isChasingTarget)
        {
            seeker.StartPath(Rb.position, target.position, OnPathComplete);
        }
    }

    private void Awake()
    {
        GameObject parentObject = GameObject.Find("Game Manager");  
        if (parentObject == null)
        {
            Debug.Log("CRITIAL ERROR: Game Manager object not found");
            return;
        }
        gameManager = parentObject.GetComponent<GameManager>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        Coroutine spawnCoroutine = StartCoroutine(PerformSpawn());
        damageVFX = GetComponent<DamageEffect>();

        if (!IsServer)
        {
            return;
        }

        target = FindTarget();
        seeker = GetComponent<Seeker>();
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        distanceToTarget = Vector2.Distance(Rb.position, target.position);
        if (distanceToTarget <= TargetDetectionDistance)
        {
            isChasingTarget = true;
        }

        PerformDeath();
        AnimateEnemy();
    }

    private void FixedUpdate()
    {
        if(!IsServer)
        {
            return;
        }

        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        direction = ((Vector2)path.vectorPath[currentWaypoint] - Rb.position).normalized;
        Vector2 force = MovementSpeed * Time.deltaTime * direction;
        Rb.AddForce(force);

        float distance = Vector2.Distance(Rb.position, path.vectorPath[currentWaypoint]);

        if (distance < NextWaypointDistance)
        {
            currentWaypoint++;
        }
    }
}
