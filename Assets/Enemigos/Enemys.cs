using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Enemys : MonoBehaviour
{
    [Header("General Settings")]
    public ZombieState currentState = ZombieState.patrolling;
    public ZombieAttackType attackType;

    [Header("Detection & Attack")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float patrolRadius = 10f;
    private bool isPatrolling;

    [Space]

    public float attackSpeed;

    private NavMeshAgent agent;
    private Vector3 patrolCenter;
    private Vector3 patrolTarget;
    private Transform targetPlayer = null;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolCenter = transform.position;
        SetNewPatrolTarget();
    }

    private void LateUpdate()
    {
        CheckState();
        Debug.Log(currentState);
    }

    private void CheckState()
    {
        switch (currentState)
        {
            case ZombieState.patrolling:
                CheckForPlayers();
                if (isPatrolling) break;
                StartCoroutine(Patrol());
                break;
            case ZombieState.alert:
                MoveToSound();
                CheckForPlayers();
                break;
            case ZombieState.following:
                FollowPlayer();
                break;
            case ZombieState.attacking:
                Attack();
                break;
            case ZombieState.dead:
                break;
            default:
                break;
        }
    }

    public void ChangeState(ZombieState newState)
    {
        switch (newState)
        {
            case ZombieState.patrolling:
                CheckForPlayers();
                StartCoroutine(Patrol());
                break;
            case ZombieState.alert:
                MoveToSound();
                CheckForPlayers();
                break;
            case ZombieState.following:
                FollowPlayer();
                break;
            case ZombieState.attacking:
                Attack();
                break;
            case ZombieState.dead:
                break;
            default:
                break;
        }
        currentState = newState;
    }

    void CheckForPlayers()
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (Collider col in playersInRange)
        {
            if (col.CompareTag("Player"))
            {
                if (targetPlayer == null)
                {
                    targetPlayer = col.transform; // Guarda al primer jugador detectado
                }

                if (Vector3.Distance(transform.position, targetPlayer.position) <= attackRange)
                {
                    ChangeState(ZombieState.attacking);
                }
                else if (Vector3.Distance(transform.position, targetPlayer.position) <= detectionRange)
                {
                    ChangeState(ZombieState.following);
                }
                return;
            }
        }

        if (targetPlayer != null) // Si el jugador sale del rango
        {
            targetPlayer = null;
            ChangeState(ZombieState.patrolling);
        }
    }

    void FollowPlayer()
    {
        if (targetPlayer == null) return;

        agent.speed = chaseSpeed;
        agent.SetDestination(targetPlayer.position);

        if (Vector3.Distance(transform.position, targetPlayer.position) > detectionRange)
        {
            targetPlayer = null;
            ChangeState(ZombieState.patrolling);
        }
        else if (Vector3.Distance(transform.position, targetPlayer.position) <= attackRange)
        {
            ChangeState(ZombieState.attacking);
        }
    }

    void Attack()
    {
        agent.isStopped = true;
        Debug.Log("¡Atacando a " + targetPlayer.name + "!");
        targetPlayer.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth);
        playerHealth.GetDamage(5);

        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        if (distance > attackRange)
        {
            agent.isStopped = false;
            ChangeState(ZombieState.following);
        }
    }

    IEnumerator Patrol()
    {
        isPatrolling = true;
        SetNewPatrolTarget();
        float patrolWaitTime = Random.Range(1, 4);
        agent.speed = patrolSpeed;

        agent.SetDestination(patrolTarget);

        yield return new WaitWhile(() => Vector3.Distance(transform.position, patrolTarget) > 1);
        
        yield return new WaitForSeconds(patrolWaitTime);
        StartCoroutine(Patrol());
    }

    void SetNewPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += patrolCenter;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
        }
    }

    public void AlertFromSound(Vector3 soundPosition)
    {
        if (currentState == ZombieState.patrolling)
        {
            patrolTarget = soundPosition;
            ChangeState(ZombieState.alert);
        }
    }

    void MoveToSound()
    {
        agent.SetDestination(patrolTarget);

        if (Vector3.Distance(transform.position, patrolTarget) < 2f)
        {
            ChangeState(ZombieState.patrolling);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckForPlayers();
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, attackRange);
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, Vector3.up, detectionRange);
        Handles.color = Color.blue;
        Handles.DrawWireDisc(transform.position, Vector3.up, patrolRadius);
    }
#endif


}

public enum ZombieState
{
    patrolling,
    alert,
    following,
    attacking,
    dead,
    searching
}

public enum ZombieAttackType
{
    melee,
    range,
    jumper,
    builder,
    healer
}