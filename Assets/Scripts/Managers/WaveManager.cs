using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    private int currentWaveId = 1; // Начальная волна. Потом необходимо устанавливать нужную волну.
    private bool isWaveActive = false;

    public bool IsWaveActive => isWaveActive;

    public Transform spawnPoint;

    public void StartWave()
    {
        // Если волна уже идет, ничего не делаем
        if (isWaveActive) return;

        // Запускаем волну как корутину
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        Debug.Log($"Wave {currentWaveId} started!");
        isWaveActive = true;
        EventHub.OnWaveStarted?.Invoke();

        WaveDataSO currentWaveData = GameData.Instance.GetWaveData(currentWaveId);

        if (currentWaveData == null)
        {
            Debug.LogError($"No data for wave {currentWaveId} found!");
            yield break;
        }

        // Проходим по КАЖДОМУ типу врагов в волне
        foreach (WaveEnemy waveEnemy in currentWaveData.enemiesInWave)
        {
            // Запускаем корутину для спавна одного типа врагов
            yield return StartCoroutine(SpawnEnemyTypeRoutine(waveEnemy));
        }

        // TODO: Здесь позже будем ждать, пока ВСЕ враги не умрут или не дойдут до базы
        yield return new WaitForSeconds(5f);

        CompleteWave();
    }

    private IEnumerator SpawnEnemyTypeRoutine(WaveEnemy waveEnemy)
    {
        for (int i = 0; i < waveEnemy.count; i++)
        {
            SpawnEnemy(waveEnemy.enemyData);
            // Ждем указанный в SO интервал
            yield return new WaitForSeconds(waveEnemy.spawnInterval);
        }
    }

    private void SpawnEnemy(EnemyDataSO enemyData)
    {
        // 1. Берем префаб из данных врага
        GameObject enemyPrefab = enemyData.prefab;
        // 2. Создаем экземпляр на сцене в точке спавна
        GameObject enemyInstance = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        // 3. Получаем компонент поведения врага
        EnemyBehaviour enemyBehaviour = enemyInstance.GetComponent<EnemyBehaviour>();
        // 4. Передаем ему его же данные для инициализации
        enemyBehaviour.SetData(enemyData);
        // 5. Передаем путь для движения
        enemyBehaviour.SetPath(Waypoints.Points);

        Debug.Log("Spawned: " + enemyData.enemyName);
    }

    private void CompleteWave()
    {
        isWaveActive = false;
        Debug.Log($"Wave {currentWaveId} completed!");

        // Сообщаем всем системам о завершении волны
        EventHub.OnWaveCompleted?.Invoke();

        // Готовимся к следующей волне
        currentWaveId++;
    }
}
