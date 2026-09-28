using UnityEngine;
using TMPro;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Wave Settings")]
    public int enemyCount = 5;
    public int enemyIncreasePerWave = 2;
    public float spawnDelay = 2f;
    public float timeBetweenWaves = 3f;

    [Header("Wave")]
    public int currentWave = 0;

    [Header("Wave UI")]
    public TextMeshProUGUI waveText;

    private int aliveEnemy = 0;
    private bool spawning = false;

    void Start()
    {
        // ซ่อนข้อความตอนเริ่ม
        if (waveText != null)
        {
            waveText.text = "";
        }

        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {
        // รอก่อนเริ่ม Wave
        yield return new WaitForSeconds(timeBetweenWaves);

        currentWave++;

        // แสดงเลข Wave บน Canvas
        if (waveText != null)
        {
            waveText.text = "WAVE " + currentWave;
        }

        // เพิ่มจำนวนมอนตาม Wave
        int totalEnemy = enemyCount +
                         ((currentWave - 1) * enemyIncreasePerWave);

        Debug.Log("===== WAVE " + currentWave + " =====");
        Debug.Log("Enemy: " + totalEnemy);

        spawning = true;

        // Spawn มอนทีละตัว
        for (int i = 0; i < totalEnemy; i++)
        {
            SpawnEnemy();

            yield return new WaitForSeconds(spawnDelay);
        }

        spawning = false;

        // ตรวจสอบว่า Wave จบหรือยัง
        CheckWaveComplete();
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("ยังไม่ได้ใส่ Enemy Prefab!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("ไม่มี Spawn Point!");
            return;
        }

        // สุ่มจุดเกิด
        int randomIndex = Random.Range(0, spawnPoints.Length);

        Transform spawnPoint = spawnPoints[randomIndex];

        // สร้าง Enemy
        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        aliveEnemy++;

        // เพิ่มตัวตรวจจับการตายให้ Enemy
        EnemyDeathTracker tracker = enemy.AddComponent<EnemyDeathTracker>();

        tracker.spawner = this;
    }

    public void EnemyDied()
    {
        aliveEnemy--;

        if (aliveEnemy < 0)
        {
            aliveEnemy = 0;
        }

        Debug.Log("Enemy เหลือ: " + aliveEnemy);

        CheckWaveComplete();
    }

    void CheckWaveComplete()
    {
        // ถ้ายัง Spawn ไม่ครบ ห้ามเริ่ม Wave ใหม่
        if (spawning)
        {
            return;
        }

        // ถ้า Enemy ตายหมด
        if (aliveEnemy <= 0)
        {
            Debug.Log("Wave " + currentWave + " Complete!");

            StartCoroutine(StartWave());
        }
    }


    // =========================================================
    // ตัวตรวจจับ Enemy ตาย
    // ไม่ต้องสร้าง Script แยก
    // =========================================================

    public class EnemyDeathTracker : MonoBehaviour
    {
        public EnemySpawner spawner;

        private bool counted = false;

        void OnDestroy()
        {
            if (counted)
            {
                return;
            }

            counted = true;

            if (spawner != null)
            {
                spawner.EnemyDied();
            }
        }
    }
}