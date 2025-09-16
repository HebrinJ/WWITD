using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    private int currentWaveId = 1; // ��������� �����. ����� ���������� ������������� ������ �����.
    private bool isWaveActive = false;

    public bool IsWaveActive => isWaveActive;

    public Transform spawnPoint;

    public void StartWave()
    {
        // ���� ����� ��� ����, ������ �� ������
        if (isWaveActive) return;

        // ��������� ����� ��� ��������
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

        // �������� �� ������� ���� ������ � �����
        foreach (WaveEnemy waveEnemy in currentWaveData.enemiesInWave)
        {
            // ��������� �������� ��� ������ ������ ���� ������
            yield return StartCoroutine(SpawnEnemyTypeRoutine(waveEnemy));
        }

        // TODO: ����� ����� ����� �����, ���� ��� ����� �� ����� ��� �� ������ �� ����
        yield return new WaitForSeconds(5f);

        CompleteWave();
    }

    private IEnumerator SpawnEnemyTypeRoutine(WaveEnemy waveEnemy)
    {
        for (int i = 0; i < waveEnemy.count; i++)
        {
            SpawnEnemy(waveEnemy.enemyData);
            // ���� ��������� � SO ��������
            yield return new WaitForSeconds(waveEnemy.spawnInterval);
        }
    }

    private void SpawnEnemy(EnemyDataSO enemyData)
    {
        // 1. ����� ������ �� ������ �����
        GameObject enemyPrefab = enemyData.prefab;
        // 2. ������� ��������� �� ����� � ����� ������
        GameObject enemyInstance = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        // 3. �������� ��������� ��������� �����
        EnemyBehaviour enemyBehaviour = enemyInstance.GetComponent<EnemyBehaviour>();
        // 4. �������� ��� ��� �� ������ ��� �������������
        enemyBehaviour.SetData(enemyData);
        // 5. �������� ���� ��� ��������
        enemyBehaviour.SetPath(Waypoints.Points);

        Debug.Log("Spawned: " + enemyData.enemyName);
    }

    private void CompleteWave()
    {
        isWaveActive = false;
        Debug.Log($"Wave {currentWaveId} completed!");

        // �������� ���� �������� � ���������� �����
        EventHub.OnWaveCompleted?.Invoke();

        // ��������� � ��������� �����
        currentWaveId++;
    }
}
