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

    private void InitializeResources()
    {
        resources[ResourceType.Money] = startMoney;
        resources[ResourceType.Steel] = startSteel;
        resources[ResourceType.Fuel] = startFuel;

        foreach (var resource in resources)
        {
            EventHub.OnResourceAmountChanged?.Invoke(resource.Key, resource.Value);
        }
    }

    // Обработка запроса на трату ресурсов
    private void HandleSpendRequest(ResourceCost cost)
    {
        if (CanAfford(cost))
        {
            SpendResource(cost.resourceType, cost.amount);
            //EventHub.OnTowerBuildConfirmed?.Invoke(null, Vector3.zero); // TODO: передавать реальные данные
        }
        else
        {
            Debug.Log($"Not enough {cost.resourceType}!");
            // Можно вызвать событие OnNotEnoughResources
        }
    }

    // Обработка добавления ресурсов
    private void HandleResourceAdded(ResourceType type, int amount)
    {
        AddResource(type, amount);
    }

    // Обработка смерти врага - награда деньгами
    private void HandleEnemyDied(EnemyBehaviour enemy)
    {
        if (enemy != null && enemy.Data != null)
        {
            AddResource(ResourceType.Money, enemy.Data.rewardMoney);
        }
    }

    // Проверка, хватает ли ресурсов
    public bool CanAfford(ResourceCost cost)
    {
        return resources.ContainsKey(cost.resourceType) &&
               resources[cost.resourceType] >= cost.amount;
    }

    // Проверка, хватает ли ресурсов (удобная перегрузка)
    public bool CanAfford(ResourceType type, int amount)
    {
        return CanAfford(new ResourceCost(type, amount));
    }

    private void SpendResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] -= amount;
            EventHub.OnResourceAmountChanged?.Invoke(type, resources[type]);
            Debug.Log($"Spent {amount} {type}. Remaining: {resources[type]}");
        }
    }

    // Добавление ресурсов
    private void AddResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] += amount;
            EventHub.OnResourceAmountChanged?.Invoke(type, resources[type]);
            Debug.Log($"Added {amount} {type}. Total: {resources[type]}");
        }
    }

    // Получение текущего количества ресурса (для UI)
    public int GetResourceAmount(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }
}
