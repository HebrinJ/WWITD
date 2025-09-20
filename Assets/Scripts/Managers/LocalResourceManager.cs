using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��������, ���������� �� ��������, ������������ � �������� � ���������� ��������� � ������ ������ ������������ ������.
/// ������������ ���������� � �������� ��������, ��������� ����������� ���������� ���������� � ���������� ������ ������� �� ����������.
/// </summary>
/// <remarks>
/// <para><b>��������� vs. ���������� �������:</b> ��������� ������ ���������, ������������� �� ������ (������, ������, �������).
/// �� ��������� ����������� ��������� (Blueprints), ������� �������������� ��������� ��������.</para>
/// <para><b>���� � �����������:</b> �������� �������� ����������� �� �������, ��������� � ���������� �������� �� ������.</para>
/// </remarks>
public class LocalResourceManager : MonoBehaviour
{
    /// <summary>
    /// ������� ��� �������� �������� ���������� ������� ���� ���������� �������.
    /// </summary>
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    [SerializeField]
    [Tooltip("��������� ���������� ����� �� ������.")]
    private int startMoney = 100;

    [SerializeField]
    [Tooltip("��������� ���������� ������ �� ������.")]
    private int startIron = 50;

    [SerializeField]
    [Tooltip("��������� ���������� �������� �� ������.")]
    private int startFuel = 25;

    /// <summary>
    /// �������������� ��������� �������� �������� ��� ������� ������ � ��������� UI �� �� �������.
    /// </summary>
    private void Start()
    {
        InitializeResources();
    }

    private void OnEnable()
    {
        EventHub.OnLocalResourceSpendRequested += HandleSpendRequest;
        EventHub.OnLocalResourceAdded += HandleResourceAdded;
        EventHub.OnEnemyDiedForMoney += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EventHub.OnLocalResourceSpendRequested -= HandleSpendRequest;
        EventHub.OnLocalResourceAdded -= HandleResourceAdded;
        EventHub.OnEnemyDiedForMoney -= HandleEnemyDied;
    }

    /// <summary>
    /// ����� ������������� �������� � ������ ������. ��������� ������� ���������� ����������
    /// � ���������� ��������� ��� ���������������� ������� (� ������ ������� UI) � ������� ���������� ������� �������.
    /// </summary>
    private void InitializeResources()
    {
        resources[ResourceType.Money] = startMoney;
        resources[ResourceType.Iron] = startIron;
        resources[ResourceType.Fuel] = startFuel;

        // ��������� UI � ��������� ��������� ������� �������
        foreach (var resource in resources)
        {
            EventHub.OnLocalResourceChanged?.Invoke(resource.Key, resource.Value);
        }
    }

    /// <summary>
    /// ���������� ������� ������� �� �������� ��������. ���������� ������� ��������� (��������, BuildManager) ��� ������� �������.
    /// ���������, ���������� �� ��������, � ���� �� - ��������� ��.
    /// </summary>
    /// <param name="cost">���������, ���������� ��� ������� � ���������� ��� ��������.</param>
    private void HandleSpendRequest(ResourceCost cost)
    {
        if (CanAfford(cost))
        {
            SpendResource(cost.resourceType, cost.amount);
        }
        else
        {
            Debug.LogWarning($"[LocalResourceManager] ������������ {cost.resourceType} ��� �������� {cost.amount}!");
            // TODO: ��������, ����� ������� ��������� ������� � ��������� ���������� (e.g., OnSpendFailed)
        }
    }

    /// <summary>
    /// ���������� ������� ������� ���������� ��������. ���������� ��� ���������� �������� ����� (��������, ��� ������ �� �������).
    /// </summary>
    /// <param name="type">��� ������������ �������.</param>
    /// <param name="amount">���������� ������������ �������.</param>
    private void HandleResourceAdded(ResourceType type, int amount)
    {
        AddResource(type, amount);
    }

    /// <summary>
    /// ���������� ������� ������ �����. ��������� �������� ������� �� ��������, ���� ������ ����� �������� ���������� � �������.
    /// </summary>
    /// <param name="enemy">��������� (Behaviour) ������� �����, ���������� ������ �� ��� ������ (Data).</param>
    private void HandleEnemyDied(EnemyBehaviour enemy)
    {
        if (enemy != null && enemy.Data != null)
        {
            AddResource(ResourceType.Money, enemy.Data.rewardMoney);
        }
    }

    /// <summary>
    /// ��������� ����� ��� ��������, ���������� �� �������� ��� �������� ��������� ���������.
    /// </summary>
    /// <param name="cost">���������, ���������� ��� ������� � ��������� ����������.</param>
    /// <returns>True - ���� �������� ����������, False - � ��������� ������.</returns>
    public bool CanAfford(ResourceCost cost)
    {
        return resources.ContainsKey(cost.resourceType) && resources[cost.resourceType] >= cost.amount;
    }

    /// <summary>
    /// ������� ���������� ������ �������� ��� ������� �������� ���� � ����������.
    /// </summary>
    /// <param name="type">��� ������� ��� ��������.</param>
    /// <param name="amount">��������� ���������� �������.</param>
    /// <returns>True - ���� �������� ����������, False - � ��������� ������.</returns>
    public bool CanAfford(ResourceType type, int amount)
    {
        return CanAfford(new ResourceCost(type, amount));
    }

    /// <summary>
    /// ���������� ����� ��� �������� ��������. ��������� ������� ������� � ��������� ������� �� ���������.
    /// ���������� ����� �������� �������� CanAfford.
    /// </summary>
    /// <param name="type">��� ������������ �������.</param>
    /// <param name="amount">���������� ������������ �������.</param>
    private void SpendResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] -= amount;
            EventHub.OnLocalResourceChanged?.Invoke(type, resources[type]);
        }
    }

    /// <summary>
    /// ���������� ����� ��� ���������� ��������. ����������� ������� ������� � ��������� ������� �� ���������.
    /// </summary>
    /// <param name="type">��� ������������ �������.</param>
    /// <param name="amount">���������� ������������ �������.</param>
    private void AddResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] += amount;
            EventHub.OnLocalResourceChanged?.Invoke(type, resources[type]);
            EventHub.OnLocalResourceAdded?.Invoke(type, amount);
        }
    }

    /// <summary>
    /// ��������� ������ ��� ��������� �������� ���������� ����������� �������.
    /// </summary>
    /// <param name="type">��� �������������� �������.</param>
    /// <returns>������� ���������� �������. ���� ������ �� ������, ���������� 0.</returns>
    public int GetResourceAmount(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }
}