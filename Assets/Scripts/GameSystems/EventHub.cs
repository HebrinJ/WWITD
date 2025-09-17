using System;
using UnityEngine;

public static class EventHub
{
    // === ���������� ������� (Blueprints) ===
    public static Action<ResourceType, int> OnGlobalResourceChanged;
    public static Action<ResourceType, int> OnGlobalResourceAdded;
    public static Action<ResourceCost> OnGlobalResourceSpendRequested; // ��� ������������ �� �����

    // === ��������� ������� (Money, Steel, Fuel) ===
    public static Action<ResourceType, int> OnLocalResourceChanged;
    public static Action<ResourceType, int> OnLocalResourceAdded;
    public static Action<ResourceCost> OnLocalResourceSpendRequested; // ��� ������������� �� ������
    public static Action<EnemyBehaviour> OnEnemyDiedForMoney; // ������� ��� ������� ��������


    // �������������
    public static Action<TowerPlaceholder> OnTowerPlaceholderSelected; // ������� ����� ��� �������������
    public static Action<TowerType> OnTowerSelectedFromUI; // ������� ��� ����� �� UI
    public static Action<TowerDataSO, Vector3> OnTowerBuildConfirmed;
    public static Action OnTowerBuildFailed;
    public static Action OnConstructionModeToggled; // ����� ������ ������ �������������
    public static Action<bool> OnConstructionModeChanged; // ����� ����� ���������
    public static Action<TowerBehaviour> OnTowerOptionsRequested; // �������� ����� �����

    // ����� � �����
    public static Action OnWaveStarted;
    public static Action OnWaveCompleted;
    public static Action<EnemyBehaviour> OnEnemyReachedBase;

    // ������� �������
    public static Action OnGameStart;
    public static Action OnGameOver;
    public static Action<EnemyBehaviour> OnEnemyDied;
    public static Action OnLevelComplete;
}