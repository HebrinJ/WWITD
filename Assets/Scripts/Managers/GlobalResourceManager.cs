using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��������-��������, ���������� �� �������� � ���������� ����������� ��������� ������ (���������),
/// ������� ����������� ����� �������� � ������������ ��� ������������ �� �������������� �����.
/// </summary>
/// <remarks>
/// <para><b>���������� vs. ��������� �������:</b> ��������� ������ ���������, ������������ ����� �������� (� ��������� ����� ������ �������).
/// �� ��������� ���������� ��������� ������ (������, ������, �������), ������� �������������� <see cref="LocalResourceManager"/>.</para>
/// <para><b>������� Singleton:</b> ���������� ��� ��������, ����������� ����� ������� (<see cref="DontDestroyOnLoad"/>),
/// ��� ������������ ������ ����� ������� � ���������� �������� �� ����� ����� (������� ����, �����, �������).</para>
/// <para><b>���� � �����������:</b> ������������ ������������� ������ ��� ������� ������������. ������������ ����������,
/// ��������� � �������� ������������ �� �������������� �����.</para>
/// </remarks>
public class GlobalResourceManager : MonoBehaviour
{
    /// <summary>
    /// ����������� �������� ��� ����������� ������� � ���������� ���������.
    /// ��������� ������ Singleton.
    /// </summary>
    public static GlobalResourceManager Instance { get; private set; }

    /// <summary>
    /// ������� ��� �������� �������� ���������� ������� ���� ����������� �������.
    /// � ������� ���������� �������������� �������� ������ ResourceType.Blueprints.
    /// </summary>
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    [SerializeField]
    [Tooltip("��������� ���������� �������� � ����� ����.")]
    private int startBlueprints = 0;

    /// <summary>
    /// ����� ������������� ���������.
    /// ���������� ������������� ���������� � �������� ������ ��������� ��� ���������� ����� �������.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeResources();
    }

    private void OnEnable()
    {
        EventHub.OnGlobalResourceSpendRequested += HandleSpendRequest;
        EventHub.OnGlobalResourceAdded += HandleResourceAdded;
    }

    private void OnDisable()
    {
        EventHub.OnGlobalResourceSpendRequested -= HandleSpendRequest;
        EventHub.OnGlobalResourceAdded -= HandleResourceAdded;
    }

    /// <summary>
    /// ������������� ��������� �������� ���������� ��������.
    /// � ������� ���������� �������������� ������ �������.
    /// </summary>
    private void InitializeResources()
    {
        resources[ResourceType.Blueprints] = startBlueprints;
        EventHub.OnGlobalResourceChanged?.Invoke(ResourceType.Blueprints, resources[ResourceType.Blueprints]);
    }

    /// <summary>
    /// ���������� ������� ������� �� �������� ���������� ��������.
    /// ���������� ���������, ���������� �������� � �������� �������� (��������, ResearchManager ��� ������� ������������).
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
            Debug.LogWarning($"[GlobalResourceManager] ������������ ����������� ������� {cost.resourceType}!");
            // TODO: ������� ������� OnGlobalSpendFailed ��� UI
        }
    }

    /// <summary>
    /// ���������� ������� ������� ���������� ���������� ��������.
    /// ���������� ��� ���������� �������� (��������, ��� ���������� ������ ��� ���������� �������).
    /// </summary>
    /// <param name="type">��� ������������ �������.</param>
    /// <param name="amount">���������� ������������ �������.</param>
    private void HandleResourceAdded(ResourceType type, int amount)
    {
        AddResource(type, amount);
    }

    /// <summary>
    /// ��������� ����� ��� �������� ����������� �������� ��������� ���������.
    /// </summary>
    /// <param name="cost">���������, ���������� ��� ������� � ��������� ����������.</param>
    /// <returns>True - ���� �������� ����������, False - � ��������� ������.</returns>
    public bool CanAfford(ResourceCost cost)
    {
        return resources.ContainsKey(cost.resourceType) && resources[cost.resourceType] >= cost.amount;
    }

    /// <summary>
    /// ���������� ����� ��� �������� ���������� ��������.
    /// ��������� ������� ������� � ��������� ������� �� ���������.
    /// </summary>
    /// <param name="type">��� ������������ �������.</param>
    /// <param name="amount">���������� ������������ �������.</param>
    private void SpendResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] -= amount;
            EventHub.OnGlobalResourceChanged?.Invoke(type, resources[type]);
        }
    }

    /// <summary>
    /// ��������� ����� ��� ���������� ���������� ��������.
    /// ������������ ��� ���������� ������ �� ������, ������� � ��.
    /// </summary>
    /// <param name="type">��� ������������ �������.</param>
    /// <param name="amount">���������� ������������ �������.</param>
    public void AddResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] += amount;
            EventHub.OnGlobalResourceChanged?.Invoke(type, resources[type]);
            EventHub.OnGlobalResourceAdded?.Invoke(type, amount);
        }
        else
        {
            // ���� ������ ����� �� ��� ���������������, �������������� ���
            resources[type] = amount;
            EventHub.OnGlobalResourceChanged?.Invoke(type, resources[type]);
            EventHub.OnGlobalResourceAdded?.Invoke(type, amount);
        }
    }

    /// <summary>
    /// ��������� ������ ��� ��������� �������� ���������� ����������� ����������� �������.
    /// </summary>
    /// <param name="type">��� �������������� �������.</param>
    /// <returns>������� ���������� �������. ���� ������ �� ������, ���������� 0.</returns>
    public int GetResourceAmount(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }
}