using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float startHealth = 100f;
    public float currentHealth;

    // ส่งค่าไปให้ UI เมื่อเลือดเปลี่ยน
    public event Action<float, float> OnHealthChanged;

    private bool isDead = false;

    void Awake()
    {
        currentHealth = Mathf.Clamp(startHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("PLAYER HP: " + currentHealth + " / " + maxHealth);

        // แจ้ง UI ว่าเลือดเปลี่ยน
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public bool IsDead()
    {
        return isDead;
    }

    void Die()
    {
        isDead = true;

        Debug.Log("PLAYER DIED!");
    }

    public void FullHeal()
    {
        isDead = false;
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}