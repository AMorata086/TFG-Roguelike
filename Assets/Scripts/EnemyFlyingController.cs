using Pathfinding;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemyFlyingController : NetworkBehaviour
{
    /* VARIABLES */
    // Enemy stats
    [Header("Enemy Stats")]
    public int HealthPoints = 5;
    public int Damage = 1;
    public float MovementSpeed = 3500f;
    public float BulletSpeed = 500f;
    public float ShootingCooldown = 0.5f;
    private float lastShotTime = 0;
    public float MaxShootingDistance = 4f;
    private bool isShooting = false;

    public GameObject BulletPrefab;
    public Rigidbody2D Rb;
    public GameObject AimPoint;

    public Animator Animator;
    [SerializeField] private ParticleSystem deathVFX;
    [SerializeField] private ParticleSystem explosionVFX;
    [SerializeField] private SpawnEffect spawnEffect;
    private DamageEffect damageVFX;

    private GameManager gameManager;

    // AI Pathfinding variables
    [Header("AI Pathfinding Variables")]
    private Seeker seeker;
    private Transform target;
    [SerializeField] private float NextWaypointDistance = 1f;
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;

    private Vector2 direction;
    private float distanceToTarget;
    private bool isChasingTarget = false;
    public float TargetDetectionDistance = 15f;

    private void Shoot()
    {
        if ((Time.time - lastShotTime) < ShootingCooldown)
        {
            return;
        }
        Vector2 shootingDirection = ((Vector2)target.position - (Vector2)AimPoint.transform.position).normalized;
        // Use the Atan2() function in order to get the angle in radians that tan(y/x) forms and convert it to degrees
        float shootingAngle = Mathf.Atan2(shootingDirection.y, shootingDirection.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(BulletPrefab, AimPoint.transform.position, Quaternion.Euler(new Vector3(0f, 0f, shootingAngle)));
        bullet.GetComponent<Rigidbody2D>().AddForce(shootingDirection * BulletSpeed * Time.fixedDeltaTime, ForceMode2D.Impulse);
        bullet.GetComponent<EnemyBulletScript>().Damage = Damage;
        lastShotTime = Time.time;
    }

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

    private GameObject FindTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

        if (targets == null)
        {   // Control if there are any errors when searching for a target
            return null;
        }
        return targets[Random.Range(0, targets.Length)];
    }

    private void AnimateEnemy()
    {
        Vector2 lookingDirection = (Vector2)target.position - (Vector2)Rb.position;
        Animator.SetBool("Moving", isChasingTarget);
        Animator.SetFloat("Direction", lookingDirection.x);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 1;
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
    void Start()
    {
        Coroutine spawnCoroutine = StartCoroutine(PerformSpawn());

        target = FindTarget().transform;
        seeker = GetComponent<Seeker>();
        damageVFX = GetComponent<DamageEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        distanceToTarget = Vector2.Distance(Rb.position, target.position);

        if (distanceToTarget > MaxShootingDistance && distanceToTarget <= TargetDetectionDistance)
        {
            isChasingTarget = true;
            isShooting = false;
        }
        else if (distanceToTarget <= MaxShootingDistance)
        {
            isChasingTarget = false;
            isShooting = true;
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

        if (isChasingTarget)
        {
            direction = ((Vector2)path.vectorPath[currentWaypoint] - Rb.position).normalized;
            Vector2 force = MovementSpeed * Time.deltaTime * direction;
            Rb.AddForce(force);
        }

        if (isShooting)
        {
            Shoot();
        }

        float distance = Vector2.Distance(Rb.position, path.vectorPath[currentWaypoint]);

        if (distance < NextWaypointDistance)
        {
            currentWaypoint++;
        }
    }
}
