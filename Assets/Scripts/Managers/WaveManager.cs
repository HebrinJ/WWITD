using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private WaveDataSO[] levelWaves;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private LevelManager levelManager;

    private int currentWaveIndex = -1;
    private bool isWavesSequenceActive = false;
    private List<EnemyBehaviour> allActiveEnemies = new List<EnemyBehaviour>();
    private List<Coroutine> activeWaveCoroutines = new List<Coroutine>();
    private int wavesCompleted = 0;

    public bool IsWavesSequenceActive => isWavesSequenceActive;
    public int TotalWaves => levelWaves != null ? levelWaves.Length : 0;
    public int CurrentWaveNumber => currentWaveIndex + 1;

    public Action<float> OnNextWaveTimerStarted;
    public Action<float> OnNextWaveTimerUpdated;
    public Action OnNextWaveTimerEnded;
    public Action<int> OnWaveStarted;
    public Action<int> OnWaveCompleted;

    private void OnEnable()
    {
        EventHub.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EventHub.OnEnemyDied -= HandleEnemyDied;
    }

    public void StartWaves()
    {
        if (isWavesSequenceActive || currentWaveIndex >= TotalWaves) return;

        isWavesSequenceActive = true;
        currentWaveIndex = 0;
        wavesCompleted = 0;

        StartWave(0);
    }

    private void StartWave(int waveIndex)
    {
        if (waveIndex >= TotalWaves) return;

        WaveDataSO waveData = levelWaves[waveIndex];
        currentWaveIndex = waveIndex;

        Debug.Log($"Starting wave {CurrentWaveNumber}");
        OnWaveStarted?.Invoke(CurrentWaveNumber);
        EventHub.OnWaveStarted?.Invoke();

        // ��������� �������� ��� ������ �����
        Coroutine waveCoroutine = StartCoroutine(SpawnWaveRoutine(waveData, waveIndex));
        activeWaveCoroutines.Add(waveCoroutine);

        // ���� ���� ��������� �����, ��������� ������ ��� ���
        if (waveIndex < TotalWaves - 1)
        {
            float nextWaveDelay = waveData.nextWaveDelay;
            Debug.Log($"Next wave will start in {nextWaveDelay} seconds");
            StartCoroutine(StartNextWaveTimerRoutine(nextWaveDelay, waveIndex + 1));
        }
    }

    private IEnumerator SpawnWaveRoutine(WaveDataSO waveData, int waveIndex)
    {
        List<EnemyBehaviour> waveEnemies = new List<EnemyBehaviour>();

        // ������� ���� ������ �����
        foreach (WaveEnemy waveEnemy in waveData.enemiesInWave)
        {
            for (int i = 0; i < waveEnemy.count; i++)
            {
                GameObject enemyInstance = Instantiate(waveEnemy.enemyData.prefab, spawnPoint.position, spawnPoint.rotation);
                EnemyBehaviour enemyBehaviour = enemyInstance.GetComponent<EnemyBehaviour>();
                enemyBehaviour.SetData(waveEnemy.enemyData);
                enemyBehaviour.SetPath(Waypoints.Points);

                waveEnemies.Add(enemyBehaviour);
                allActiveEnemies.Add(enemyBehaviour);

                yield return new WaitForSeconds(waveEnemy.spawnInterval);
            }
        }

        Debug.Log($"Wave {waveIndex + 1} spawning completed. Waiting for enemies...");

        // ���� ���� ��� ����� ���� ����� �� ����� ����������
        yield return WaitForWaveEnemiesDefeated(waveEnemies);

        // ����� ���������
        Debug.Log($"Wave {waveIndex + 1} completed");
        wavesCompleted++;
        OnWaveCompleted?.Invoke(waveIndex + 1);
        EventHub.OnWaveCompleted?.Invoke();

        // ��������� ���������� ������
        CheckLevelCompletion();
    }

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

        // ��������� ��������� �����
        StartWave(nextWaveIndex);
    }

    private IEnumerator WaitForWaveEnemiesDefeated(List<EnemyBehaviour> waveEnemies)
    {
        while (waveEnemies.Count > 0)
        {
            // ������� ������������ ������
            for (int i = waveEnemies.Count - 1; i >= 0; i--)
            {
                if (waveEnemies[i] == null)
                {
                    waveEnemies.RemoveAt(i);
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void CheckLevelCompletion()
    {
        // ������� ��������, �����:
        // 1. ��� ����� �������� (currentWaveIndex ������ ���������)
        // 2. ��� ����� ��������� (wavesCompleted == TotalWaves)
        // 3. ��� ����� ���������� (allActiveEnemies ����)

        if (currentWaveIndex >= TotalWaves - 1 &&
            wavesCompleted == TotalWaves &&
            allActiveEnemies.Count == 0)
        {
            Debug.Log("Level completed! All waves finished and all enemies defeated.");
            isWavesSequenceActive = false;
            levelManager?.CompleteLevel(true);
        }
    }

    private void HandleEnemyDied(EnemyBehaviour enemy)
    {
        if (allActiveEnemies.Contains(enemy))
        {
            allActiveEnemies.Remove(enemy);
        }
        EventHub.OnEnemyDiedForMoney?.Invoke(enemy);

        // ��������� ���������� ������ ����� ������� ��������
        CheckLevelCompletion();
    }

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

    public string GetWaveInfo()
    {
        if (currentWaveIndex < 0) return "Ready to start";
        return $"Wave {Mathf.Min(CurrentWaveNumber, TotalWaves)}/{TotalWaves}";
    }
}