using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;

    [Header("UI")]
    public Slider healthBar;
    public Slider staminaBar;

    [Header("Stamina")]
    public float staminaUse = 20f;
    public float staminaRegen = 15f;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }
    }

    void Update()
    {
        // Stamina
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
        {
            currentStamina -= staminaUse * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegen * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(
            currentStamina,
            0f,
            maxStamina
        );

        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }
    }

    // =========================
    // รับ Damage จาก Enemy
    // =========================

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0f,
            maxHealth
        );

        // อัปเดตหลอดเลือด
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        Debug.Log("Player โดนตี! เลือดเหลือ: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("PLAYER ตาย!");
    }
}