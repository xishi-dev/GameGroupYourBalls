using UnityEngine;

public class Stamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;

    [Header("Sprint Settings")]
    public float staminaDrain = 20f;
    public float staminaRegen = 15f;
    public float regenDelay = 1f;

    private float lastSprintTime;
    private bool isExhausted = false;

    void Awake()
    {
        currentStamina = maxStamina;
    }

    void Update()
    {
        RegenerateStamina();
    }

    // ใช้ตอนวิ่ง
    public bool UseStamina(float amount)
    {
        if (isExhausted)
            return false;

        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        lastSprintTime = Time.time;

        if (currentStamina <= 0f)
        {
            currentStamina = 0f;
            isExhausted = true;
        }

        return true;
    }

    // เริ่มฟื้น Stamina
    void RegenerateStamina()
    {
        if (Time.time < lastSprintTime + regenDelay)
            return;

        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegen * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

            if (currentStamina > 0f)
            {
                isExhausted = false;
            }
        }
    }

    // เช็กว่า Stamina ยังเหลือไหม
    public bool CanSprint()
    {
        return currentStamina > 0f && !isExhausted;
    }

    // เติมเต็ม Stamina
    public void FullStamina()
    {
        currentStamina = maxStamina;
        isExhausted = false;
    }
}