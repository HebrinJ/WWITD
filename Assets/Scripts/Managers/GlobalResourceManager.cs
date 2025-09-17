using System.Collections.Generic;
using UnityEngine;

public class GlobalResourceManager : MonoBehaviour
{
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    [SerializeField] private int startBlueprints = 0; // Стартовые чертежи

    private void Awake()
    {
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

    private void InitializeResources()
    {
        resources[ResourceType.Blueprints] = startBlueprints;
        EventHub.OnGlobalResourceChanged?.Invoke(ResourceType.Blueprints, resources[ResourceType.Blueprints]);
    }

    private void HandleSpendRequest(ResourceCost cost)
    {
        if (CanAfford(cost))
        {
            SpendResource(cost.resourceType, cost.amount);
        }
        else
        {
            Debug.Log($"Not enough global resource {cost.resourceType}!");
        }
    }

    private void HandleResourceAdded(ResourceType type, int amount)
    {
        AddResource(type, amount);
    }

    public bool CanAfford(ResourceCost cost)
    {
        return resources.ContainsKey(cost.resourceType) && resources[cost.resourceType] >= cost.amount;
    }

    private void SpendResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] -= amount;
            EventHub.OnGlobalResourceChanged?.Invoke(type, resources[type]);
        }
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] += amount;
            EventHub.OnGlobalResourceChanged?.Invoke(type, resources[type]);
        }
    }

    public int GetResourceAmount(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }
}