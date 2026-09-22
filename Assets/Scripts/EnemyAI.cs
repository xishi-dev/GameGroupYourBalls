using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    [Header("Detection Settings")]
    public float sightRange = 12f;          
    public float fieldOfView = 90f;        
    public LayerMask obstacleMask;         

    [Header("Patrol Settings")]
    public float walkRadius = 8f;          
    public float patrolWaitTime = 2f;      
    private float waitTimer = 0f;

    [Header("Movement Speeds")]
    public float patrolSpeed = 2f;         
    public float chaseSpeed = 4.5f;       

    private bool playerInSight = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        agent.speed = patrolSpeed;
        SetRandomDestination();
    }

    void Update()
    {
        if (player == null) return;

        CheckForPlayer();

        if (playerInSight)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void CheckForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= sightRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < fieldOfView / 2f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    playerInSight = true;
                    return;
                }
            }
        }

        playerInSight = false;
    }

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                SetRandomDestination();
                waitTimer = 0f;
            }
        }
    }

    void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}