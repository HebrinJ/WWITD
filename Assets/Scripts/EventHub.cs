using Mono.Cecil;
using System;
using UnityEngine;

public static class EventHub
{    
    // �������
    public static Action<ResourceType, int> OnResourceAmountChanged; // ��� ������� � ����� ����������
    public static Action<ResourceType, int> OnResourceAmountAdded;   // ����� ������ � ������� ��������
    public static Action<ResourceCost> OnResourceSpendRequested;     // ������ �� ����� ��������


    // �������������
    public static Action<TowerPlaceholder> OnTowerPlaceholderSelected; // ������� ����� ��� �������������
    public static Action<TowerType> OnTowerSelectedFromUI; // ������� ��� ����� �� UI
    public static Action<TowerDataSO, Vector3> OnTowerBuildConfirmed;
    public static Action OnTowerBuildFailed;

    // ����� � �����
    public static Action OnWaveStarted;
    public static Action OnWaveCompleted;
    public static Action<EnemyBehaviour> OnEnemyReachedBase;

    // ������� �������
    public static Action OnGameStart;
    public static Action OnGameOver;
    public static Action<EnemyBehaviour> OnEnemyDied;
}