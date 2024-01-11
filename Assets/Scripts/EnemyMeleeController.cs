using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEditor;

public class EnemyMeleeController : MonoBehaviour
{
    /* VARIABLES */
    // Enemy Stats
    public int HealthPoints = 15;
    public int Damage = 5;
    public float MovementSpeed = 2500f;
    public Rigidbody2D Rb;

    public Animator Animator;

    // AI Pathfinding variables
    Transform target;
    public float NextWaypointDistance = 1f;
    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    Seeker seeker;

    Vector2 direction;
    float distanceToTarget;
    bool isChasingTarget = false;
    public float TargetDetectionDistance = 10f;

    private GameObject FindTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

        if (targets == null)
        {   // Control if there are any errors when searching for a target
            return null;
        }
        return targets[Random.Range(0, targets.Length)];
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 1;
        }
    }

    void AnimateEnemy()
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
        InvokeRepeating(nameof(UpdatePath), 0f, 0.1f);
    }

    void Update()
    {
        distanceToTarget = Vector2.Distance(Rb.position, target.position);
        if (distanceToTarget <= TargetDetectionDistance)
        {
            isChasingTarget = true;
        }
        AnimateEnemy();    
    }

    void FixedUpdate()
    {
        if(path == null)
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
