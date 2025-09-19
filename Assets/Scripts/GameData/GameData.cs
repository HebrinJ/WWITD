using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    public List<WaveDataSO> allWaveData;
    public List<EnemyDataSO> allEnemyData;
    public List<TowerDataSO> allTowerData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public WaveDataSO GetWaveData(string waveId)
    {
        return allWaveData.Find(wave => wave.waveId == waveId);
    }

    public EnemyDataSO GetEnemyDataByName(string name)
    {
        return allEnemyData.Find(data => data.enemyName == name);
    }
    public TowerDataSO GetTowerData(TowerType towerType)
    {
        return allTowerData.Find(data => data.towerType == towerType);
    }
}
