using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Централизованный реестр и менеджер всех игровых данных (ScriptableObjects).
/// Реализует шаблон Singleton для обеспечения глобального доступа к данным в игре.
/// Служит единой точкой входа для поиска и доступа к конфигурациям врагов, башен и волн.
/// </summary>
public class GameData : MonoBehaviour
{
    /// <summary>
    /// Глобальная точка доступа к экземпляру GameData.
    /// Гарантирует, что в сцене существует только один экземпляр менеджера данных.
    /// </summary>
    public static GameData Instance { get; private set; }

    /// <summary>
    /// Коллекция всех WaveDataSO, используемых в игре.
    /// Должна быть заполнена через инспектор Unity ссылками на все SO волн.
    /// </summary>
    [Tooltip("Список всех WaveDataSO в игре. Заполняется через инспектор.")]
    public List<WaveDataSO> allWaveData;

    /// <summary>
    /// Коллекция всех EnemyDataSO, используемых в игре.
    /// Должна быть заполнена через инспектор Unity ссылками на все SO врагов.
    /// </summary>
    [Tooltip("Список всех EnemyDataSO в игре. Заполняется через инспектор.")]
    public List<EnemyDataSO> allEnemyData;

    /// <summary>
    /// Коллекция всех TowerDataSO, используемых в игре.
    /// Должна быть заполнена через инспектор Unity ссылками на все SO башен.
    /// </summary>
    [Tooltip("Список всех TowerDataSO в игре. Заполняется через инспектор.")]
    public List<TowerDataSO> allTowerData;

    /// <summary>
    /// Инициализирует singleton-экземпляр при создании объекта.
    /// Уничтожает дубликаты для соблюдения паттерна Singleton.
    /// </summary>
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

    /// <summary>
    /// Находит и возвращает данные волны по её уникальному идентификатору.
    /// </summary>
    /// <param name="waveId">Идентификатор волны для поиска.</param>
    /// <returns>Найденный WaveDataSO или null, если волна с таким ID не найдена.</returns>
    public WaveDataSO GetWaveData(string waveId)
    {
        return allWaveData.Find(wave => wave.waveId == waveId);
    }

    /// <summary>
    /// Находит и возвращает данные врага по его имени.
    /// </summary>
    /// <param name="name">Имя врага для поиска (соответствует полю enemyName в EnemyDataSO).</param>
    /// <returns>Найденный EnemyDataSO или null, если враг с таким именем не найден.</returns>
    public EnemyDataSO GetEnemyDataByName(string name)
    {
        return allEnemyData.Find(data => data.enemyName == name);
    }

    /// <summary>
    /// Находит и возвращает данные башни по её типу.
    /// </summary>
    /// <param name="towerType">Тип башни для поиска.</param>
    /// <returns>Найденный TowerDataSO или null, если башня такого типа не найдена.</returns>
    public TowerDataSO GetTowerData(TowerType towerType)
    {
        return allTowerData.Find(data => data.towerType == towerType);
    }
}