using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Graneat : NetworkBehaviour
{
    public float explosionDelay = 3f;
    public float explosionRadius = 5f;
    public int explosionDamage = 100;
    public GameObject explosionEffect; // Puedes asignar una partícula aquí

    private bool exploded = false;

    void Start()
    {
        StartCoroutine(ExplodeAfterDelay());
    }

    IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Buscar todos los colliders en el radio de la explosión
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.TryGetComponent<NetworkIdentity>(out var identity))
            {
                if (identity.TryGetComponent<IEnemyHealth>(out IEnemyHealth enemy))
                {
                    enemy.TakeDamage(explosionDamage);
                    enemy.FlashOnHit(); // opcional
                }
            }
        }

        // Destruir la granada después de explotar
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

}
