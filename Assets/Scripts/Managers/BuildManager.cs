using UnityEngine;

/// <summary>
/// ����������� ��������, ���������� �� ������� ������������� � ���������� ������� �� ������� ������.
/// ������������ �������������� ����� ������� ������, ��������� ��������, ������� ����� � �� ��������� � ����.
/// �������� �������� ����������� � ������������ �������, ��������� �� ��������������.
/// </summary>
/// <remarks>
/// <para><b>��������������:</b></para>
/// <list type="bullet">
/// <item><description>������������� �� ������� UI: <see cref="EventHub.OnTowerPlaceholderSelected"/>, <see cref="EventHub.OnTowerSelectedFromUI"/>.</description></item>
/// <item><description>��������������� � <see cref="LocalResourceManager"/> ��� �������� � �������� ��������.</description></item>
/// <item><description>����������� ������ ����� ����� <see cref="GameData.Instance"/>.</description></item>
/// <item><description>������� ���������� ����� � ��������� � ��� ������� ������������ ����� <see cref="ResearchEffectsManager"/>.</description></item>
/// </list>
/// </remarks>
public class BuildManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("������ �� �������� ��������� �������� (������, ������, �������) ��� �������� ��������� �������������.")]
    private LocalResourceManager localResourceManager;

    /// <summary>
    /// ������� ��������� ������� ����������� ��� �������������. 
    /// �������� ����� ������� OnTowerPlaceholderSelected � ��������� ����� ��������� ��� ������.
    /// </summary>
    private TowerPlaceholder selectedPlaceholder;

    /// <summary>
    /// ��� �����, ��������� ��� ��������� � ������� ������������.
    /// �������� ����� ������� OnTowerSelectedFromUI � ��������� ����� ��������� ��� ���� �������� ������������.
    /// </summary>
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

    /// <summary>
    /// ���������� ������� ������ ������������. ��������� ��������� ����������� ��� ������������ �������������.
    /// </summary>
    /// <param name="placeholder">��������� �����������, ���������� ���������� � ������� � ������������.</param>
    private void HandlePlaceholderSelected(TowerPlaceholder placeholder)
    {
        selectedPlaceholder = placeholder;
    }

    /// <summary>
    /// ���������� ������� ������ ���� ����� �� UI. ���������� ������� �������� � ������� ���������.
    /// ��� ������ ������� �������������� ��������� �����������.
    /// </summary>
    /// <param name="towerType">��� �����, ��������� �������.</param>
    private void HandleTowerSelected(TowerType towerType)
    {
        if (selectedPlaceholder != null)
        {
            pendingTowerType = towerType;
            TryBuildTower(towerType);
        }
    }

    /// <summary>
    /// �������� ����� ������� ��������� �����. ��������� ����������� ������ � ��������.
    /// ���� �������� �������� �������, ��������� ������� � �������� ����� ���������������� ���������.
    /// </summary>
    /// <param name="towerType">��� ����� ��� ���������.</param>
    private void TryBuildTower(TowerType towerType)
    {
        // 1. ��������� ������ ����� �� ������������ �������
        TowerDataSO towerData = GameData.Instance.GetTowerData(towerType);

        if (towerData == null)
        {
            Debug.LogError("[BuildManager] ������ ����� �� ������� ��� ����: " + towerType);
            return;
        }

        // 2. �������� ���� ��������� �������� �� ������ ���������
        bool canAffordAll = true;
        foreach (TowerCost cost in towerData.constructionCost)
        {
            if (localResourceManager != null && !localResourceManager.CanAfford(cost.resourceType, cost.amount))
            {
                canAffordAll = false;
                Debug.Log($"[BuildManager] ������������ {cost.resourceType}! ���������: {cost.amount}");
                // TODO: ������� EventHub.OnTowerBuildFailed � ��������� ������� (���� ������� X)
                break;
            }
        }

        // 3. ���� �������� ������� - ������� �� � ��������� �����
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
            Debug.Log("[BuildManager] ������������ �������� ��� ��������� ���� �����");
            pendingTowerType = TowerType.None;
            EventHub.OnTowerBuildFailed?.Invoke(); // ���������� UI � �������
        }
    }

    /// <summary>
    /// ���������� ������� ������������� ���������. ��������������.
    /// � ������� ���������� ����� ����� �� UI ����� ���������� ���������.
    /// </summary>
    /// <param name="data">������ �����.</param>
    /// <param name="position">������� ��� ���������.</param>
    private void HandleBuildConfirmed(TowerDataSO data, Vector3 position)
    {
        // ������ ��� ������� ������� ������������� ���������
        // Debug.Log("Build confirmed event received. Future implementation.");
    }

    /// <summary>
    /// ��������� ����� ���������������� ��������� ����� � ����.
    /// ������� ��������� �������, ����������� ��� � ������������ �������������� �����������.
    /// </summary>
    /// <param name="towerData">������ ���������� �����.</param>
    private void BuildTower(TowerDataSO towerData)
    {
        if (selectedPlaceholder != null)
        {
            InstantiateTower(towerData, selectedPlaceholder.transform.position);

            // ������ placeholder ���������� ����� ���������
            selectedPlaceholder.gameObject.SetActive(false);
            selectedPlaceholder = null;
            pendingTowerType = TowerType.None;
        }
    }

    /// <summary>
    /// ������� ��������� ����� � ����, �������������� ��� ������ � ��������� ������������� ���������.
    /// </summary>
    /// <param name="towerData">ScriptableObject � ������� �����.</param>
    /// <param name="position">������� � ������� ������������ ��� �������� �����.</param>
    private void InstantiateTower(TowerDataSO towerData, Vector3 position)
    {
        // 1. �������� ���������� ������� �����
        GameObject towerInstance = Instantiate(towerData.towerPrefab, position, Quaternion.identity);
        Debug.Log($"[BuildManager] ��������� �����: {towerData.towerName}");

        // 2. ������������� ������ �����
        TowerBehaviour towerBehaviour = towerInstance.GetComponent<TowerBehaviour>();
        if (towerBehaviour != null)
        {
            towerBehaviour.SetData(towerData);
        }
        else
        {
            Debug.LogError("[BuildManager] � ������� ����� ����������� ��������� TowerBehaviour: " + towerData.towerName);
        }

        // 3. ���������� �������� �� ������������ (���������) � ����� �����
        if (ResearchEffectsManager.Instance != null)
        {
            ResearchEffectsManager.Instance.ApplyResearchEffectsToTower(towerBehaviour);
        }
        else
        {
            Debug.LogWarning("[BuildManager] ResearchEffectsManager �� ������! ����� ��������� ��� ����� ������������.");
        }
    }
}