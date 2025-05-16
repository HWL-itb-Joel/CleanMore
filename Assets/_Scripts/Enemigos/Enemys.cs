using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Enemys : NetworkBehaviour, IEnemyHealth
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
    private bool isAttacking;

    [SerializeField] private GameObject particles;
    [SerializeField] private Transform posParticle;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health;

    public int Health
    {
        get => health;
        set => health = value;
    }

    public int zombieHealth;
    public float attackDamage = 5f;

    [Space]

    public float attackSpeed;

    private NavMeshAgent agent;
    private Vector3 patrolCenter;
    private Vector3 patrolTarget;
    private Transform targetPlayer = null;
    [SerializeField] private Animator animator;

    public Renderer enemyRenderer;
    public Material originalMaterial;
    public Material flashMaterial;
    public float flashDuration = 0.1f;

    private Coroutine flashCoroutine;


    void Start()
    {
        Health = zombieHealth;
        agent = GetComponent<NavMeshAgent>();

        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{gameObject.name} no está sobre el NavMesh.");
            enabled = false;
            return;
        }

        patrolCenter = transform.position;
    }

    private void OnEnable()
    {
        SetNewPatrolTarget();
    }

    private void LateUpdate()
    {
        if (Health <= 0)
        {
            ChangeState(ZombieState.dead);
            return;
        }
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
                if (isAttacking) break;
                StartCoroutine(Attack());
                break;
            case ZombieState.dead:
                GameObject a = Instantiate(particles);
                a.transform.position = transform.position;
                Destroy(a,1.5f);
                gameObject.SetActive(false);
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
                animator.SetBool("FollowPlayer", false);
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
                animator.SetBool("Walking", true);
                animator.SetBool("FollowPlayer", true);
                StartCoroutine(Attack());
                break;
            case ZombieState.dead:
                GameObject a = Instantiate(particles);
                a.transform.position = posParticle.position;
                a.transform.localScale = posParticle.localScale;
                Destroy(a, 1.5f);
                gameObject.SetActive(false);
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
                    animator.SetBool("Walking", true);
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
        if (targetPlayer == null) 
        {
            animator.SetBool("FollowPlayer", false);
            return;
        }

        animator.SetBool("FollowPlayer", true);

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

    IEnumerator Attack()
    {
        animator.SetBool("Walking", false);
        animator.SetTrigger("Attack");
        agent.isStopped = true;
        isAttacking = true;
        Debug.Log("¡Atacando a " + targetPlayer.name + "!");
        targetPlayer.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth);
        playerHealth.GetDamage(attackDamage);
        playerHealth.GetComponentInChildren<GunController>().StartFlashFeedBack();

        yield return new WaitForSeconds(1f);

        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        if (distance > attackRange)
        {
            agent.isStopped = false;
            animator.SetBool("Walking", true);
            ChangeState(ZombieState.following);
            isAttacking = false;
            yield return null;
        }
        isAttacking = false;
    }

    IEnumerator Patrol()
    {
        if (isPatrolling)
            yield break;

        isPatrolling = true;
        animator.SetBool("Walking", true);
        SetNewPatrolTarget();

        agent.speed = patrolSpeed;
        agent.SetDestination(patrolTarget);

        yield return new WaitUntil(() => Vector3.Distance(transform.position, patrolTarget) < 1);

        animator.SetBool("Walking", false);
        yield return new WaitForSeconds(Random.Range(4, 10));

        isPatrolling = false;
        ChangeState(ZombieState.patrolling); // vuelve a comenzar si es necesario
    }


    void SetNewPatrolTarget()
    {
        int maxTries = 10;
        int attempts = 0;
        while (attempts < maxTries)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += patrolCenter;
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                var path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    patrolTarget = hit.position;
                    return;
                }
            }
            attempts++;
        }
        Debug.LogWarning("SetNewPatrolTarget falló al encontrar una ruta válida.");
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

    public void OnHitFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        enemyRenderer.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        enemyRenderer.material = originalMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckForPlayers();
    }

    public void FlashOnHit()
    {
        StartCoroutine(FlashRoutine());
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
    }

    public void OnHealthChanged(int oldValue, int newValue)
    {
        // Lógica de sincronización visual o eventos
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