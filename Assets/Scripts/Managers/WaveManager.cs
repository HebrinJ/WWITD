using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Менеджер, отвечающий за управление последовательностью волн врагов на тактическом уровне.
/// Загружает данные волн, спавнит врагов по заданному сценарию, отслеживает прогресс прохождения волн
/// и предоставляет UI-события для отображения таймеров и номера текущей волны.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является "постановщиком" врагов на уровне. Работает с данными (WaveDataSO)
/// и управляет временем их появления. Не отвечает за условия победы/поражения уровня в целом - только оповещает
/// о завершении всех волн через событие <see cref="OnAllWavesCompleted"/>.</para>
/// </remarks>
public class WaveManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Массив данных волн для этого уровня. Заполняется в инспекторе.")]
    private WaveDataSO[] levelWaves;

    [SerializeField]
    [Tooltip("Точка на сцене, где появляются враги.")]
    private Transform spawnPoint;

    private int currentWaveIndex = -1;
    private bool isWavesSequenceActive = false;
    private List<EnemyBehaviour> allActiveEnemies = new List<EnemyBehaviour>();
    private List<Coroutine> activeWaveCoroutines = new List<Coroutine>();
    private Dictionary<EnemyBehaviour, List<EnemyBehaviour>> enemyToWaveMap = new Dictionary<EnemyBehaviour, List<EnemyBehaviour>>();
    private int wavesCompleted = 0;

    /// <summary>
    /// Флаг, указывающий, активна ли в данный момент последовательность волн.
    /// </summary>
    public bool IsWavesSequenceActive => isWavesSequenceActive;

    /// <summary>
    /// Общее количество волн на уровне.
    /// </summary>
    public int TotalWaves => levelWaves != null ? levelWaves.Length : 0;

    /// <summary>
    /// Порядковый номер текущей волны (начиная с 1).
    /// </summary>
    public int CurrentWaveNumber => currentWaveIndex + 1;

    // События для UI
    public Action<float> OnNextWaveTimerStarted;
    public Action<float> OnNextWaveTimerUpdated;
    public Action OnNextWaveTimerEnded;
    public Action<int> OnWaveStarted;
    public Action<int> OnWaveCompleted;

    /// <summary>
    /// Событие, вызываемое когда все волны уровня пройдены и все враги уничтожены.
    /// Оповещает о выполнении условия "победы по волнам". Окончательное решение о победе
    /// на уровне принимает <see cref="LevelManager"/>.
    /// </summary>
    public Action OnAllWavesCompleted;

    private void OnEnable()
    {
        EventHub.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EventHub.OnEnemyDied -= HandleEnemyDied;
    }

    /// <summary>
    /// Публичный метод для запуска всей последовательности волн уровня.
    /// Обычно вызывается по нажатию кнопки "Start Wave" после фазы подготовки.
    /// </summary>
    public void StartWaves()
    {
        if (isWavesSequenceActive || levelWaves == null || currentWaveIndex >= TotalWaves) return;

        isWavesSequenceActive = true;
        currentWaveIndex = 0;
        wavesCompleted = 0;
        allActiveEnemies.Clear();

        StartWave(0);
    }

    /// <summary>
    /// Внутренний метод для запуска конкретной волны по индексу.
    /// </summary>
    /// <param name="waveIndex">Индекс волны в массиве levelWaves.</param>
    private void StartWave(int waveIndex)
    {
        if (waveIndex >= TotalWaves) return;

        WaveDataSO waveData = levelWaves[waveIndex];
        currentWaveIndex = waveIndex;

        Debug.Log($"[WaveManager] Запускается волна {CurrentWaveNumber}");
        OnWaveStarted?.Invoke(CurrentWaveNumber);
        EventHub.OnWaveStarted?.Invoke();

        // Запускаем корутину для спавна волны
        Coroutine waveCoroutine = StartCoroutine(SpawnWaveRoutine(waveData, waveIndex));
        activeWaveCoroutines.Add(waveCoroutine);

        // Если есть следующая волна, запускаем таймер для нее
        if (waveIndex < TotalWaves - 1)
        {
            float nextWaveDelay = waveData.nextWaveDelay;
            Debug.Log($"[WaveManager] Следующая волна через {nextWaveDelay} секунд.");
            StartCoroutine(StartNextWaveTimerRoutine(nextWaveDelay, waveIndex + 1));
        }
    }

    /// <summary>
    /// Корутина, осуществляющая спавн всех врагов в волне с заданными интервалами.
    /// </summary>
    /// <param name="waveData">Данные волны (WaveDataSO).</param>
    /// <param name="waveIndex">Индекс волны.</param>
    /// <returns>IEnumerator для работы корутины.</returns>
    private IEnumerator SpawnWaveRoutine(WaveDataSO waveData, int waveIndex)
    {
        List<EnemyBehaviour> waveEnemies = new List<EnemyBehaviour>();

        // Спавним всех врагов волны согласно настройкам
        foreach (WaveEnemy waveEnemy in waveData.enemiesInWave)
        {
            for (int i = 0; i < waveEnemy.count; i++)
            {
                GameObject enemyInstance = Instantiate(waveEnemy.enemyData.prefab, spawnPoint.position, spawnPoint.rotation);
                EnemyBehaviour enemyBehaviour = enemyInstance.GetComponent<EnemyBehaviour>();
                if (enemyBehaviour != null)
                {
                    waveEnemies.Add(enemyBehaviour);
                    allActiveEnemies.Add(enemyBehaviour);
                    enemyBehaviour.SetData(waveEnemy.enemyData);
                    enemyBehaviour.SetPath(Waypoints.Instance.PathPoints);
                    enemyToWaveMap.Add(enemyBehaviour, waveEnemies);
                }
                yield return new WaitForSeconds(waveEnemy.spawnInterval);
            }
        }

        Debug.Log($"[WaveManager] Спавн волны {waveIndex + 1} завершен. Ожидание уничтожения врагов...");

        // Ждем, пока все враги этой волны не будут уничтожены или не достигнут базы
        yield return WaitForWaveEnemiesDefeated(waveEnemies);

        // Волна завершена
        Debug.Log($"[WaveManager] Волна {waveIndex + 1} пройдена.");
        wavesCompleted++;
        OnWaveCompleted?.Invoke(waveIndex + 1);
        EventHub.OnWaveCompleted?.Invoke();

        // Проверяем, были ли это последняя волна и последние враги
        CheckAllWavesCompletion();
    }

    /// <summary>
    /// Корутина для обратного отсчета до следующей волны.
    /// </summary>
    /// <param name="delay">Задержка до следующей волны в секундах.</param>
    /// <param name="nextWaveIndex">Индекс следующей волны.</param>
    /// <returns>IEnumerator для работы корутины.</returns>
    private IEnumerator StartNextWaveTimerRoutine(float delay, int nextWaveIndex)
    {
        OnNextWaveTimerStarted?.Invoke(delay);

        float timer = delay;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            OnNextWaveTimerUpdated?.Invoke(timer);
            yield return null;
        }

        OnNextWaveTimerEnded?.Invoke();
        StartWave(nextWaveIndex); // Запускаем следующую волну
    }

    /// <summary>
    /// Вспомогательная корутина для ожидания уничтожения всех врагов текущей волны.
    /// </summary>
    /// <param name="waveEnemies">Список врагов текущей волны.</param>
    /// <returns>IEnumerator для работы корутины.</returns>
    private IEnumerator WaitForWaveEnemiesDefeated(List<EnemyBehaviour> waveEnemies)
    {
        // Просто ждем, пока список волны не опустеет.
        // HandleEnemyDied теперь сам удаляет врагов из этого списка.
        while (waveEnemies.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Проверяет, были ли пройдены все волны и уничтожены все враги.
    /// Если да, оповещает о завершении всей последовательности волн.
    /// </summary>
    private void CheckAllWavesCompletion()
    {
        // Условие: все волны запущены, все волны пройдены, все враги уничтожены
        if (currentWaveIndex >= TotalWaves - 1 &&
            wavesCompleted == TotalWaves &&
            allActiveEnemies.Count == 0)
        {
            Debug.Log("[WaveManager] Все волны пройдены и все враги уничтожены.");
            isWavesSequenceActive = false;
            OnAllWavesCompleted?.Invoke(); // Оповещаем о завершении всех волн
        }
    }

    /// <summary>
    /// Обработчик события смерти врага. Удаляет врага из списка активных и инициирует начисление награды.
    /// </summary>
    /// <param name="enemy">Поведение убитого врага.</param>
    private void HandleEnemyDied(EnemyBehaviour enemy)
    {
        // Удаляем врага из общего списка активных врагов
        if (allActiveEnemies.Contains(enemy))
        {
            allActiveEnemies.Remove(enemy);
        }

        // КРИТИЧЕСКИ ВАЖНО: Удаляем врага из списка его волны
        if (enemyToWaveMap.ContainsKey(enemy))
        {
            List<EnemyBehaviour> waveList = enemyToWaveMap[enemy];
            waveList.Remove(enemy);
            enemyToWaveMap.Remove(enemy); // Убираем запись из словаря
        }

        // Инициируем начисление награды
        EventHub.OnEnemyDiedForMoney?.Invoke(enemy);
        // Проверяем завершение ВСЕХ волн после каждого убийства
        CheckAllWavesCompletion();
    }

    /// <summary>
    /// Принудительно останавливает все активные корутины волн и сбрасывает состояние менеджера.
    /// </summary>
    public void StopAllWaves()
    {
        foreach (var coroutine in activeWaveCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeWaveCoroutines.Clear();
        isWavesSequenceActive = false;
    }

    /// <summary>
    /// Вспомогательный метод для получения текстовой информации о текущей волне.
    /// </summary>
    /// <returns>Строка с информацией о прогрессе волн.</returns>
    public string GetWaveInfo()
    {
        if (currentWaveIndex < 0) return "Готово к запуску";
        return $"Волна {Mathf.Min(CurrentWaveNumber, TotalWaves)}/{TotalWaves}";
    }
}