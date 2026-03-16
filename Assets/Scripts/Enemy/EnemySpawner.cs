using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки вторжения")]
    [Tooltip("Список всех доступных ScriptableObject врагов")]
    public List<EnemyData> enemyTypes;

    [Tooltip("Точки спавна врагов")]
    public Transform[] spawnPoints;

    [Header("Тайминги в секундах")]
    public float minSpawnDelay = 120f;
    public float maxSpawnDelay = 600f;

    [Header("Количество врагов")]
    public int minEnemyPerWave = 1;
    public int maxEnemyPerWave = 5;

    private void Start()
    {
        if (enemyTypes.Count > 0 && spawnPoints.Length > 0)
        {
            StartCoroutine(SpawnRoutine());
        }
        else
        {
            Debug.LogWarning("Спавнер не настроен: добавьте типы EnemyData и точки спавна (Transform).");
        }
    }
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnWave();

            float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
            Debug.LogWarning($"Следующая волна через {waitTime / 60f:F1} минут");

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnWave()
    {
        int enemiesToSpawn = Random.Range(minEnemyPerWave, maxEnemyPerWave + 1);
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        BaseEnemy leader = null;

        Debug.LogWarning($"Вторжение! Количество врагов: {enemiesToSpawn}. Точка: {spawnPoint.name}");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            EnemyData randomEnemyData = enemyTypes[Random.Range(0, enemyTypes.Count)];

            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);

            GameObject newEnemy = Instantiate(randomEnemyData.enemyPrefab, spawnPoint.position + offset, Quaternion.identity);
            BaseEnemy enemy = newEnemy.GetComponent<BaseEnemy>();
            if (i == 0) { enemy.isLeader = true; leader = enemy; leader.queuePosition = i; }
            else { enemy.leader = leader; enemy.queuePosition = i; }

        }
    }
}
