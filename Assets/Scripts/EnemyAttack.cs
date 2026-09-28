using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    private float nextAttackTime = 0f;

    private Transform player;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("หา Player ไม่เจอ! ตรวจสอบว่า Player มี Tag = Player");
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // ถ้า Player อยู่ในระยะโจมตี
        if (distance <= attackRange)
        {
            Attack();
        }
    }

    void Attack()
    {
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;

        PlayerStats playerStats = player.GetComponent<PlayerStats>();

        if (playerStats != null)
        {
            playerStats.TakeDamage(attackDamage);

            Debug.Log(
                "Enemy ตี Player! Damage = " + attackDamage
            );
        }
        else
        {
            Debug.LogError(
                "Player ไม่มี PlayerStats!"
            );
        }
    }
}