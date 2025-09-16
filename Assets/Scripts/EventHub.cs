using Mono.Cecil;
using System;
using UnityEngine;

public static class EventHub
{    
    // Ресурсы
    public static Action<ResourceType, int> OnResourceAmountChanged; // Тип ресурса и новое количество
    public static Action<ResourceType, int> OnResourceAmountAdded;   // Какой ресурс и сколько добавили
    public static Action<ResourceCost> OnResourceSpendRequested;     // Запрос на трату ресурсов


    // Строительство
    public static Action<TowerPlaceholder> OnTowerPlaceholderSelected; // Выбрали место для строительства
    public static Action<TowerType> OnTowerSelectedFromUI; // Выбрали тип башни из UI
    public static Action<TowerDataSO, Vector3> OnTowerBuildConfirmed;
    public static Action OnTowerBuildFailed;

    // Волны и враги
    public static Action OnWaveStarted;
    public static Action OnWaveCompleted;
    public static Action<EnemyBehaviour> OnEnemyReachedBase;

    // Игровой процесс
    public static Action OnGameStart;
    public static Action OnGameOver;
    public static Action<EnemyBehaviour> OnEnemyDied;
}