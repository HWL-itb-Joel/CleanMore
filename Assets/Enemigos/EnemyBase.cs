using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    [Header("General Settings")]
    public ZombieState currentState = ZombieState.patrolling;
    public ZombieAttackType attackType;

    [Header("Detection & Attack")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float fieldOfView = 120f;
    public float rotationSpeed = 5f; // Velocidad de giro
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;
    public float alertRadius = 15f;
    public float searchTime = 5f; // Tiempo que buscará al jugador antes de patrullar
    public bool isPatrolling;

    private NavMeshAgent agent;
    private Vector3 patrolCenter;
    private Vector3 patrolTarget;
    private Transform targetPlayer = null;
    private Vector3 lastKnownPosition;
    private float searchTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        switch (currentState)
        {
            case ZombieState.patrolling:
                CheckForPlayers();
                if (isPatrolling) break;
                StartCoroutine(Patrol());
                break;

            case ZombieState.alert:
                MoveToAlert();
                CheckForPlayers();
                break;

            case ZombieState.following:
                RotateTowardsTarget();
                FollowPlayer();
                break;

            case ZombieState.searching:
                SearchForPlayer();
                break;

            case ZombieState.attacking:
                Attack();
                break;
        }
    }

    void CheckForPlayers()
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);

        foreach (Collider col in playersInRange)
        {
            Transform player = col.transform;
            if (IsPlayerInSight(player))
            {
                if (targetPlayer == null)
                {
                    targetPlayer = player;
                    lastKnownPosition = player.position; // Guarda la última posición vista
                    currentState = ZombieState.following;
                    AlertNearbyZombies(player.position);
                }
                return;
            }
        }

        if (targetPlayer != null) // Si el jugador escapa
        {
            targetPlayer = null;
            searchTimer = searchTime;
            currentState = ZombieState.searching; // 🔥 Ahora el zombie buscará un poco antes de rendirse
        }
    }

    bool IsPlayerInSight(Transform player)
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer < fieldOfView / 2)
        {
            if (!Physics.Linecast(transform.position, player.position, obstacleLayer))
            {
                return true;
            }
        }
        return false;
    }

    void RotateTowardsTarget()
    {
        if (targetPlayer == null) return;

        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void FollowPlayer()
    {
        if (targetPlayer == null) return;

        agent.SetDestination(targetPlayer.position);
        lastKnownPosition = targetPlayer.position;

        if (!IsPlayerInSight(targetPlayer) || (Vector3.Distance(transform.position, targetPlayer.transform.position) >= detectionRange))
        {
            targetPlayer = null;
            searchTimer = searchTime;
            currentState = ZombieState.searching;
        }
    }

    void SearchForPlayer()
    {
        agent.SetDestination(lastKnownPosition);

        if (Vector3.Distance(transform.position, lastKnownPosition) < 2f)
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0)
            {
                currentState = ZombieState.patrolling;
            }
        }
    }

    void AlertNearbyZombies(Vector3 alertPosition)
    {
        Collider[] zombies = Physics.OverlapSphere(transform.position, alertRadius);
        foreach (Collider col in zombies)
        {
            if (col.CompareTag("Zombie"))
            {
                EnemyBase zombie = col.GetComponent<EnemyBase>();
                if (zombie != null && zombie.currentState == ZombieState.patrolling)
                {
                    zombie.AlertFromAnotherZombie(alertPosition);
                }
            }
        }
    }

    public void AlertFromAnotherZombie(Vector3 alertPosition)
    {
        if (currentState == ZombieState.patrolling)
        {
            agent.SetDestination(alertPosition);
            currentState = ZombieState.alert;
        }
    }

    void MoveToAlert()
    {
        if (Vector3.Distance(transform.position, agent.destination) < 2f)
        {
            currentState = ZombieState.patrolling;
        }
    }

    IEnumerator Patrol()
    {
        isPatrolling = true;
        SetNewPatrolTarget();
        float patrolWaitTime = Random.Range(1, 4);
        agent.speed = searchTime;
        Debug.Log("EMPIEZA PATRULLA");

        agent.SetDestination(patrolTarget);

        yield return new WaitWhile(() => Vector3.Distance(transform.position, patrolTarget) > 1);

        yield return new WaitForSeconds(patrolWaitTime);
        Debug.Log("Fin de patrulla");
        StartCoroutine(Patrol());
    }

    void SetNewPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * detectionRange;
        randomDirection += patrolCenter;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, detectionRange, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
        }
    }

    void Attack()
    {
        Debug.Log("¡Atacando!");
    }
}
