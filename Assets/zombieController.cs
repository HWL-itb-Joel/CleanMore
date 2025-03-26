using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class zombieController : MonoBehaviour
{
    NavMeshAgent agent = null;
    [SerializeField] Transform target;

    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
    }

    private void Update()
    {
        MoveToTarget();
    }

    private void MoveToTarget()
    {
        agent.SetDestination(target.position);
    }

    private void RotateToTarget()
    {
        transform.LookAt(target);
    }

    private void GetReferences()
    {
        agent = GetComponent<NavMeshAgent>();
    }
}
