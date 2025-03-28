using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperEnemy : Enemys
{
    public float jumpCharge;
    public float jumpForce;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Attack()
    {
        StartCoroutine(JumpAttack());
    }

    IEnumerator JumpAttack()
    {
        yield return new WaitForSeconds(jumpCharge);
        rb.AddForce(new Vector3(transform.forward.x, transform.forward.y * jumpForce, transform.forward.z));
    }
}
