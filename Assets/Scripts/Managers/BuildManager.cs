using UnityEngine;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    [SerializeField] LocalResourceManager localResourceManager;
    private TowerPlaceholder selectedPlaceholder;
    private TowerType pendingTowerType;

    private void OnEnable()
    {
        EventHub.OnTowerPlaceholderSelected += HandlePlaceholderSelected;
        EventHub.OnTowerSelectedFromUI += HandleTowerSelected;
        EventHub.OnTowerBuildConfirmed += HandleBuildConfirmed;
    }

    private void OnDisable()
    {
        EventHub.OnTowerPlaceholderSelected -= HandlePlaceholderSelected;
        EventHub.OnTowerSelectedFromUI -= HandleTowerSelected;
        EventHub.OnTowerBuildConfirmed -= HandleBuildConfirmed;
    }

    private void HandlePlaceholderSelected(TowerPlaceholder placeholder)
    {
        selectedPlaceholder = placeholder;
        Debug.Log("Placeholder selected");
    }

    private void HandleTowerSelected(TowerType towerType)
    {
        if (selectedPlaceholder != null)
        {
            pendingTowerType = towerType;
            TryBuildTower(towerType);
        }
    }

    private void TryBuildTower(TowerType towerType)
    {
        TowerDataSO towerData = GameData.Instance.GetTowerData(towerType);

        if (towerData == null)
        {
            Debug.LogError("Tower data not found for type: " + towerType);
            return;
        }

        // Проверяем все требуемые ресурсы
        bool canAffordAll = true;
        foreach (TowerCost cost in towerData.constructionCost)
        {
            if (localResourceManager != null && !localResourceManager.CanAfford(cost.resourceType, cost.amount))
            {
                canAffordAll = false;
                Debug.Log($"Not enough {cost.resourceType}! Need: {cost.amount}");
                break;
            }
        }

        if (canAffordAll)
        {
            foreach (TowerCost cost in towerData.constructionCost)
            {
                ResourceCost resourceCost = new ResourceCost(cost.resourceType, cost.amount);
                EventHub.OnLocalResourceSpendRequested?.Invoke(resourceCost);
            }
            BuildTower(towerData);
        }
        else
        {
            Debug.Log("Cannot afford this tower");
            pendingTowerType = TowerType.None;
        }
    }

    private void HandleBuildConfirmed(TowerDataSO data, Vector3 position)
    {
        // Это событие пока не используем, но оставляем для будущего
    }

    private void BuildTower(TowerDataSO towerData)
    {
        if (selectedPlaceholder != null)
        {
            InstantiateTower(towerData, selectedPlaceholder.transform.position);

            // Делаем placeholder неактивным после постройки
            selectedPlaceholder.gameObject.SetActive(false);
            selectedPlaceholder = null;
            pendingTowerType = TowerType.None;
        }
    }

    private void InstantiateTower(TowerDataSO towerData, Vector3 position)
    {
        GameObject towerInstance = Instantiate(towerData.towerPrefab, position, Quaternion.identity);
        Debug.Log("Tower built: " + towerData.towerName);

        // Передаем данные башне
        TowerBehaviour towerBehaviour = towerInstance.GetComponent<TowerBehaviour>();
        if (towerBehaviour != null)
        {
            towerBehaviour.SetData(towerData);
        }
    }
}