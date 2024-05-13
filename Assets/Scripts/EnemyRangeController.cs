using Pathfinding;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemyRangeController : NetworkBehaviour
{
    /* VARIABLES */
    // Enemy stats
    [Header("Enemy Stats")]
    public int HealthPoints = 15;
    public int Damage = 1;
    public float MovementSpeed = 2000f;
    public float BulletSpeed = 500f;
    public float ShootingCooldown = 0.5f;
    private float lastShotTime = 0;
    public float MaxShootingDistance = 4f;
    private bool isShooting = false;

    public GameObject BulletPrefab;
    public Rigidbody2D Rb;
    public GameObject AimPoint;
    Transform shootingPoint;

    public Animator Animator;
    [SerializeField] private ParticleSystem deathVFX;
    [SerializeField] private ParticleSystem explosionVFX;
    [SerializeField] private SpawnEffect spawnEffect;
    private DamageEffect damageVFX;

    private GameManager gameManager;

    // AI Pathfinding variables
    [Header("AI Pathfinding Variables")]
    Seeker seeker;
    Transform target;
    [SerializeField] private float NextWaypointDistance = 1f;
    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    Vector2 direction;
    float distanceToTarget;
    bool isChasingTarget = false;
    public float TargetDetectionDistance = 15f;

    private void Shoot()
    {
        if ((Time.time - lastShotTime) < ShootingCooldown)
        {
            return;
        }
        Vector2 shootingDirection = ((Vector2)target.position - (Vector2)shootingPoint.position).normalized;
        // Use the Atan2() function in order to get the angle in radians that tan(y/x) forms and convert it to degrees
        float shootingAngle = Mathf.Atan2(shootingDirection.y, shootingDirection.x) * Mathf.Rad2Deg;
        GameObject bullet = Instantiate(BulletPrefab, shootingPoint.position, Quaternion.Euler(new Vector3 (0f, 0f, shootingAngle)));
        bullet.GetComponent<Rigidbody2D>().AddForce(shootingDirection * BulletSpeed * Time.fixedDeltaTime, ForceMode2D.Impulse);
        bullet.GetComponent<EnemyBulletScript>().Damage = Damage;
        NetworkObject bulletGameObjectNetworkObject = bullet.GetComponent<NetworkObject>();
        bulletGameObjectNetworkObject.Spawn(true);
        CallShootingFxClientRpc();
        lastShotTime = Time.time;
    }

    [ClientRpc]
    private void CallShootingFxClientRpc()
    {
        SoundEffectManager.Instance.PlaySound(SoundEffectManager.Instance.SFXRefs.EnemyRangedShot, gameObject.transform.position);
    }

    public void GetHurt(int damageReceived)
    {
        HealthPoints -= damageReceived;
        InstantiateDamageFxClientRpc();
    }

    [ClientRpc]
    private void InstantiateDamageFxClientRpc()
    {
        damageVFX.CallDamageEffect();
        SoundEffectManager.Instance.PlaySound(SoundEffectManager.Instance.SFXRefs.EnemyHurt, gameObject.transform.position);
    }

    private IEnumerator PerformSpawn()
    {
        spawnEffect.CallSpawnEffect();
        yield return new WaitForSeconds(1);
        if (IsServer)
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
            InstantiateDeathFxClientRpc();
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    private void InstantiateDeathFxClientRpc()
    {
        ParticleSystem.Instantiate(deathVFX, gameObject.transform.position, gameObject.transform.rotation);
        ParticleSystem.Instantiate(explosionVFX, gameObject.transform.position, gameObject.transform.rotation);
        SoundEffectManager.Instance.PlaySound(SoundEffectManager.Instance.SFXRefs.EnemyDies, gameObject.transform.position);
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

    private void AnimateEnemy()
    {
        Vector2 lookingDirection = (Vector2)target.position - (Vector2)Rb.position;
        Animator.SetBool("Moving", isChasingTarget);
        Animator.SetFloat("Direction", lookingDirection.x);
        if (lookingDirection.x < 0)
        {
            AimPoint.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            AimPoint.transform.localScale = new Vector3(1, 1, 1);
        }
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
        damageVFX = GetComponent<DamageEffect>();

        if (!IsServer)
        {
            return;
        }

        target = FindTarget();
        seeker = GetComponent<Seeker>();
        shootingPoint = AimPoint.transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
        {
            return;
        }

        distanceToTarget = Vector2.Distance(Rb.position, target.position);
        if (TargetDetectionDistance < distanceToTarget)
        {
            isChasingTarget = false;
            isShooting = false;
        }
        else if ((MaxShootingDistance < distanceToTarget) && (distanceToTarget <= TargetDetectionDistance))
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

    void FixedUpdate()
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
