using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Enemys : MonoBehaviour
{
    public ZombieAttackType attackType;
    public ZombieState state;
    public float alertDistance;
    public float followDistance;
    public float attackDistance;

    public Transform target;

    private void LateUpdate()
    {
        CheckState();
    }

    private void CheckState()
    {
        switch (state)
        {
            case ZombieState.idle:
                IdleState();
                break;
            case ZombieState.patrolling:
                PatrolState();
                break;
            case ZombieState.alert:
                AlertState();
                break;
            case ZombieState.following:
                FollowingState();
                break;
            case ZombieState.attacking:
                AttackState();
                break;
            case ZombieState.dead:
                DeadState();
                break;
            default:
                break;
        }
    }

    public void ChangeState(ZombieState newState)
    {
        switch (newState)
        {
            case ZombieState.idle:
                IdleState();
                break;
            case ZombieState.patrolling:
                PatrolState();
                break;
            case ZombieState.alert:
                AlertState();
                break;
            case ZombieState.following:
                FollowingState();
                break;
            case ZombieState.attacking:
                AttackState();
                break;
            case ZombieState.dead:
                DeadState();
                break;
            default:
                break;
        }
        state = newState;
    }

    public virtual void IdleState()
    {

    }

    public virtual void PatrolState()
    {

    }

    public virtual void AlertState()
    {

    }

    public virtual void FollowingState()
    {

    }

    public virtual void AttackState()
    {

    }

    public virtual void DeadState()
    {

    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, Vector3.up, alertDistance);
        Handles.color = Color.blue;
        Handles.DrawWireDisc(transform.position, Vector3.up, followDistance);
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, attackDistance);
    }
#endif
}

public enum ZombieState
{
    idle,
    patrolling,
    alert,
    following,
    attacking,
    dead
}

public enum ZombieAttackType
{
    melee,
    range,
    jumper,
    builder,
    healer
}