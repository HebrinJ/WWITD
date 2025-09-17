using System.Collections.Generic;
using UnityEngine;

public class LocalResourceManager : MonoBehaviour
{
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    [SerializeField] private int startMoney = 100;
    [SerializeField] private int startSteel = 50;
    [SerializeField] private int startFuel = 25;

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

    private void InitializeResources()
    {
        resources[ResourceType.Money] = startMoney;
        resources[ResourceType.Steel] = startSteel;
        resources[ResourceType.Fuel] = startFuel;

        // Оповещаем UI
        foreach (var resource in resources)
        {
            EventHub.OnLocalResourceChanged?.Invoke(resource.Key, resource.Value);
        }
    }

    private void HandleSpendRequest(ResourceCost cost)
    {
        // Запрос на списание идет из локального контекста (BuildManager)
        if (CanAfford(cost))
        {
            SpendResource(cost.resourceType, cost.amount);
        }
        else
        {
            Debug.Log($"Not enough {cost.resourceType}!");
        }
    }

    private void HandleResourceAdded(ResourceType type, int amount)
    {
        AddResource(type, amount);
    }

    private void HandleEnemyDied(EnemyBehaviour enemy)
    {
        // Начисляем деньги только за смерть врага
        if (enemy != null && enemy.Data != null)
        {
            AddResource(ResourceType.Money, enemy.Data.rewardMoney);
        }
    }

    public bool CanAfford(ResourceCost cost)
    {
        return resources.ContainsKey(cost.resourceType) && resources[cost.resourceType] >= cost.amount;
    }

    public bool CanAfford(ResourceType type, int amount)
    {
        return CanAfford(new ResourceCost(type, amount));
    }

    private void SpendResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] -= amount;
            EventHub.OnLocalResourceChanged?.Invoke(type, resources[type]);
        }
    }

    private void AddResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] += amount;
            EventHub.OnLocalResourceChanged?.Invoke(type, resources[type]);
        }
    }

    public int GetResourceAmount(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }
}