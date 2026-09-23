using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Death Effect (Optional)")]
    public GameObject deathEffect;

    [Header("Drop Settings")]
    public GameObject primaryAmmoBoxPrefab;   
    public GameObject secondaryAmmoBoxPrefab; 
    [Range(0, 100)]
    public float dropChance = 70f;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (Random.Range(0f, 100f) <= dropChance)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.3f;
            GameObject dropPrefab = (Random.value > 0.5f) ? primaryAmmoBoxPrefab : secondaryAmmoBoxPrefab;

            if (dropPrefab != null)
            {
                Instantiate(dropPrefab, spawnPos, Quaternion.identity);
            }
        }

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    System.Collections.IEnumerator FlashRed()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color originalColor = rend.material.color;
            rend.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (rend != null)
            {
                rend.material.color = originalColor;
            }
        }
    }
}