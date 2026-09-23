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
        EnemyHealth enemy = collision.gameObject.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            Debug.Log("กระสุนโดนศัตรู: " + collision.gameObject.name);
            enemy.TakeDamage(damage);
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