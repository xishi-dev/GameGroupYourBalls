using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Slider hpSlider;

    void Start()
    {
        currentHealth = maxHealth;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log("Player HP: " + currentHealth);

        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("PLAYER DEAD!");

        // ตรงนี้ค่อยเพิ่ม Game Over ทีหลัง
        Time.timeScale = 0f;
    }
}