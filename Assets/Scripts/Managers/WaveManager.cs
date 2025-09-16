using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private WaveDataSO[] levelWaves;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private LevelManager levelManager;

    private int currentWaveIndex = 0;
    private bool isWaveActive = false;
    private List<EnemyBehaviour> activeEnemies = new List<EnemyBehaviour>();

    public bool IsWaveActive => isWaveActive;
    public int TotalWaves => levelWaves != null ? levelWaves.Length : 0;
    public int CurrentWaveNumber => currentWaveIndex + 1;

    private void OnEnable()
    {
        EventHub.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EventHub.OnEnemyDied -= HandleEnemyDied;
    }

    public void StartWave()
    {
        Debug.Log("Wave started");
        if (isWaveActive || currentWaveIndex >= TotalWaves) return;

        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        isWaveActive = true;
        Debug.Log($"Starting wave {CurrentWaveNumber}/{TotalWaves}");
        EventHub.OnWaveStarted?.Invoke();

        // Получаем данные текущей волны
        WaveDataSO currentWaveData = levelWaves[currentWaveIndex];

        // Спавним все группы врагов в волне
        foreach (WaveEnemy waveEnemy in currentWaveData.enemiesInWave)
        {
            yield return StartCoroutine(SpawnEnemyGroupRoutine(waveEnemy));
        }

        // Ждем, пока все враги не будут уничтожены или не дойдут до базы
        yield return WaitForWaveCompletion();

        CompleteWave();
    }

    private IEnumerator SpawnEnemyGroupRoutine(WaveEnemy waveEnemy)
    {
        for (int i = 0; i < waveEnemy.count; i++)
        {
            SpawnEnemy(waveEnemy.enemyData);
            yield return new WaitForSeconds(waveEnemy.spawnInterval);
        }
    }

    private void SpawnEnemy(EnemyDataSO enemyData)
    {
        GameObject enemyInstance = Instantiate(enemyData.prefab, spawnPoint.position, spawnPoint.rotation);
        EnemyBehaviour enemyBehaviour = enemyInstance.GetComponent<EnemyBehaviour>();
        enemyBehaviour.SetData(enemyData);
        enemyBehaviour.SetPath(Waypoints.Points);

        activeEnemies.Add(enemyBehaviour);
        Debug.Log($"Spawned: {enemyData.enemyName}");
    }

    private IEnumerator WaitForWaveCompletion()
    {
        // Ждем пока есть активные враги
        while (activeEnemies.Count > 0)
        {
            // Удаляем уничтоженных врагов из списка
            activeEnemies.RemoveAll(enemy => enemy == null);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void HandleEnemyDied(EnemyBehaviour enemy)
    {
        // Удаляем врага из списка активных
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }

    private void CompleteWave()
    {
        isWaveActive = false;
        currentWaveIndex++;

        Debug.Log($"Wave {CurrentWaveNumber - 1} completed!");
        EventHub.OnWaveCompleted?.Invoke();

        // Проверяем, была ли это последняя волна
        if (currentWaveIndex >= TotalWaves)
        {
            Debug.Log("All waves completed! Level victory!");
            levelManager?.CompleteLevel(true);
        }
    }

    // Метод для получения информации о волнах (для UI)
    public string GetWaveInfo()
    {
        return $"Wave {CurrentWaveNumber}/{TotalWaves}";
    }

    // Метод для принудительного запуска конкретной волны (для тестов)
    public void StartSpecificWave(int waveIndex)
    {
        if (waveIndex >= 0 && waveIndex < TotalWaves)
        {
            currentWaveIndex = waveIndex;
            StartWave();
        }
    }
}