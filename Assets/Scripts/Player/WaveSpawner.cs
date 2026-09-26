using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Wave Settings")]
    public int startingEnemies = 5;
    public int enemiesIncreasePerWave = 2;
    public float timeBetweenWaves = 3f;

    private int currentWave = 0;
    private int enemiesAlive = 0;

    void Start()
    {
        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        currentWave++;

        int enemiesToSpawn = startingEnemies +
                             (currentWave - 1) * enemiesIncreasePerWave;

        Debug.Log("WAVE " + currentWave +
                  " | Enemies: " + enemiesToSpawn);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();

            // เวลาระหว่างเกิดแต่ละตัว
            yield return new WaitForSeconds(0.3f);
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("ไม่มี Spawn Point!");
            return;
        }

        Transform spawnPoint =
            spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject enemy =
            Instantiate(enemyPrefab,
                        spawnPoint.position,
                        spawnPoint.rotation);

        enemiesAlive++;
    }

    public void EnemyKilled()
    {
        enemiesAlive--;

        if (enemiesAlive <= 0)
        {
            StartCoroutine(StartNextWave());
        }
    }
}