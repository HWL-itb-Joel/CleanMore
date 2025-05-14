using UnityEngine;
using System.Collections;

public class EnemyDamageFlash : MonoBehaviour
{
    public Renderer enemyRenderer;
    public Material originalMaterial;
    public Material flashMaterial;
    public float flashDuration = 0.1f;

    private Coroutine flashCoroutine;

    void Start()
    {
        // Asegúrate de tener referencias válidas
        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponentInChildren<Renderer>();
        }

        originalMaterial = enemyRenderer.material;
    }

    public void FlashOnHit()
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
}
