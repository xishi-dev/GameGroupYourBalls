using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    public float spawnInterval = 3f;
    public int maxEnemies = 10;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;

            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        // ถ้ามอนในฉากถึงจำนวนสูงสุดแล้ว ไม่เกิดเพิ่ม
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length >= maxEnemies)
            return;

        if (spawnPoints.Length == 0)
            return;

        // สุ่มจุดเกิด
        Transform spawnPoint =
            spawnPoints[Random.Range(0, spawnPoints.Length)];

        // สร้าง Enemy
        Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}