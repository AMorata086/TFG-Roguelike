using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlyingController : MonoBehaviour
{
    /* VARIABLES */
    // Enemy stats
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

    // AI Pathfinding variables
    Seeker seeker;
    Transform target;
    public float NextWaypointDistance = 1f;
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
        GameObject bullet = Instantiate(BulletPrefab, AimPoint.transform.position, AimPoint.transform.rotation);
        bullet.GetComponent<Rigidbody2D>().AddForce(((Vector2)target.position - (Vector2)AimPoint.transform.position).normalized * BulletSpeed * Time.fixedDeltaTime, ForceMode2D.Impulse);
        lastShotTime = Time.time;
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
        // TODO
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 1;
        }
    }

    void UpdatePath()
    {
        if (seeker.IsDone() && isChasingTarget)
        {
            seeker.StartPath(Rb.position, target.position, OnPathComplete);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        target = FindTarget().transform;
        seeker = GetComponent<Seeker>();
        InvokeRepeating("UpdatePath", 0f, 0.1f);
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
