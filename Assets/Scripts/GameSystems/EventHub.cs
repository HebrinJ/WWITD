using System;
using UnityEngine;

public static class EventHub
{
    // === ГЛОБАЛЬНЫЕ РЕСУРСЫ (Blueprints) ===
    public static Action<ResourceType, int> OnGlobalResourceChanged;
    public static Action<ResourceType, int> OnGlobalResourceAdded;
    public static Action<ResourceCost> OnGlobalResourceSpendRequested; // Для исследований на карте

    // === ЛОКАЛЬНЫЕ РЕСУРСЫ (Money, Steel, Fuel) ===
    public static Action<ResourceType, int> OnLocalResourceChanged;
    public static Action<ResourceType, int> OnLocalResourceAdded;
    public static Action<ResourceCost> OnLocalResourceSpendRequested; // Для строительства на уровне
    public static Action<EnemyBehaviour> OnEnemyDiedForMoney; // Событие для награды деньгами


    // Строительство
    public static Action<TowerPlaceholder> OnTowerPlaceholderSelected; // Выбрали место для строительства
    public static Action<TowerType> OnTowerSelectedFromUI; // Выбрали тип башни из UI
    public static Action<TowerDataSO, Vector3> OnTowerBuildConfirmed;
    public static Action OnTowerBuildFailed;
    public static Action OnConstructionModeToggled; // Когда нажали кнопку Строительства
    public static Action<bool> OnConstructionModeChanged; // Когда режим изменился
    public static Action<TowerBehaviour> OnTowerOptionsRequested; // Показать опции башни

    // Волны и враги
    public static Action OnWaveStarted;
    public static Action OnWaveCompleted;
    public static Action<EnemyBehaviour> OnEnemyReachedBase;

    // Игровой процесс
    public static Action OnGameStart;
    public static Action OnGameOver;
    public static Action<EnemyBehaviour> OnEnemyDied;
    public static Action OnLevelComplete;
}