using Pathfinding;
using System.Collections;
using UnityEngine;

public class EnemyMeleeController : MonoBehaviour
{
    /* VARIABLES */
    // Enemy Stats
    [Header("Enemy Stats")]
    public int HealthPoints = 10;
    [SerializeField] private int damage = 5;
    [SerializeField] private float knockbackForce = 1000f;
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
        damageVFX.CallDamageEffect();
    }

    private IEnumerator PerformSpawn()
    {
        spawnEffect.CallSpawnEffect();
        yield return new WaitForSeconds(1);
        InvokeRepeating(nameof(UpdatePath), 0f, 0.1f);
        yield return null;
    }

    private void PerformDeath()
    {
        if (HealthPoints <= 0)
        {
            gameManager.DecrementCurrentEnemies();
            ParticleSystem.Instantiate(deathVFX, gameObject.transform.position, gameObject.transform.rotation);
            ParticleSystem.Instantiate(explosionVFX, gameObject.transform.position, gameObject.transform.rotation);
            Destroy(gameObject);
        }
    }

    private void AttackTarget(Collider2D collision)
    {
        // invoke the GetHurt method from the player's controller
        collision.gameObject.GetComponentInParent<PlayerController>().GetHurt(damage);
        // Apply a knockback to the player when in contact with the enemy
        collision.gameObject.GetComponentInParent<Rigidbody2D>().AddForce(gameObject.GetComponent<Rigidbody2D>().velocity.normalized * knockbackForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Attack the player
            AttackTarget(collision);
        }
    }

    private GameObject FindTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

        if (targets == null)
        {   // Control if there are any errors when searching for a target
            return null;
        }
        return targets[Random.Range(0, targets.Length)];
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
        
        target = FindTarget().transform;
        seeker = GetComponent<Seeker>();
        damageVFX = GetComponent<DamageEffect>();
    }

    private void Update()
    {
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
