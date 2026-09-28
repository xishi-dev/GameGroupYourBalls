using UnityEngine;

public class BouncingBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 5f;
    public int maxBounces = 4;
    public float damage = 25f;

    private int bounceCount = 0;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        EnemyAI enemyAI = collision.gameObject.GetComponentInParent<EnemyAI>();
        if (enemyAI == null)
        {
            enemyAI = collision.gameObject.GetComponent<EnemyAI>();
        }

        if (enemyAI != null)
        {
            enemyAI.TakeDamage(Mathf.RoundToInt(damage));
            Destroy(gameObject);
            return;
        }

        EnemyHealth enemyHealth = collision.gameObject.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        bounceCount++;
        if (bounceCount >= maxBounces)
        {
            Destroy(gameObject);
        }
    }
}