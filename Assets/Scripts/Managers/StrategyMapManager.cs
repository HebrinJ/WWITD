using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������� �������������� �����, ���������� �� ���������� �������� ��������.
/// ������������ ����������� �������, �� ����������� � �������� ������.
/// </summary>
/// <remarks>
/// <para><b>���� � �����������:</b> ����������� ����������� ��� ���� ������ �������������� �����.
/// ��������� ���������� �������, ������������ ����� ������ � ���������������� � �������� ����������.</para>
/// <para><b>�������������� ����� EventHub:</b> ��� ����������� � ���������� ������������ ����� EventHub,
/// ��� ������������ ������ ��������� � UI � ������� ���������.</para>
/// </remarks>
public class StrategyMapManager : MonoBehaviour
{
    /// <summary>
    /// ���������� ����� ������� � ���������� ��������� �������������� �����.
    /// </summary>
    public static StrategyMapManager Instance { get; private set; }

    /// <summary>
    /// ������ ���� ������� �������� � ���������� �������.
    /// ������ ���� �������� � ���������� Unity.
    /// </summary>
    [Tooltip("������ ���� ������� ��������. ����������� � ���������� ������������������.")]
    [SerializeField] private LevelDataSO[] campaignLevels;

    /// <summary>
    /// ������� ��� �������� ������� � ������ ������ �� ��������������.
    /// </summary>
    private Dictionary<string, LevelDataSO> levelDataMap = new Dictionary<string, LevelDataSO>();

    /// <summary>
    /// ������� ��������� ������� �������.
    /// </summary>
    private LevelDataSO selectedLevel;

    private void Awake()
    {
        InitializeSingleton();
        InitializeLevelData();
    }

    private void OnEnable()
    {
        EventHub.OnSaveDataLoaded += HandleSaveDataLoaded;
        EventHub.OnLevelCompleted += HandleLevelCompleted;
    }

    private void OnDisable()
    {
        EventHub.OnSaveDataLoaded -= HandleSaveDataLoaded;
        EventHub.OnLevelCompleted -= HandleLevelCompleted;
    }

    /// <summary>
    /// �������������� singleton-��������� ���������.
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[StrategyMapManager] Singleton ���������������");
    }

    /// <summary>
    /// �������������� ������ ������� � ������� ������� ��� �������� �������.
    /// </summary>
    private void InitializeLevelData()
    {
        levelDataMap.Clear();

        if (campaignLevels == null || campaignLevels.Length == 0)
        {
            Debug.LogError("[StrategyMapManager] �� ��������� ������ ��������!");
            return;
        }

        foreach (var levelData in campaignLevels)
        {
            if (levelData != null && !string.IsNullOrEmpty(levelData.levelId))
            {
                levelDataMap[levelData.levelId] = levelData;
            }
        }

        Debug.Log($"[StrategyMapManager] ���������������� �������: {levelDataMap.Count}");
    }

    /// <summary>
    /// ���������� �������� ������ ����������.
    /// ��������� ��������� ������� ����� �������� ��������� ������.
    /// </summary>
    /// <param name="playerData">����������� ������ ������.</param>
    private void HandleSaveDataLoaded(PlayerSaveData playerData)
    {
        UpdateLevelsAvailability();
        Debug.Log("[StrategyMapManager] ��������� ������� ��������� ����� �������� ����������");
    }

    /// <summary>
    /// ���������� ���������� ������.
    /// ��������� ��������� ������ � ��������� ��������� �����.
    /// </summary>
    /// <param name="levelId">������������� ������������ ������.</param>
    private void HandleLevelCompleted(string levelId)
    {
        // ��������� ������� �� �������
        AwardLevelCompletion(levelId);

        // ��������� ����������� �������
        UpdateLevelsAvailability();

        Debug.Log($"[StrategyMapManager] ������� {levelId} ��������. ����������� ������� ���������");
    }

    /// <summary>
    /// ��������� ������� �� ���������� ������.
    /// </summary>
    /// <param name="levelId">������������� ������������ ������.</param>
    private void AwardLevelCompletion(string levelId)
    {
        if (levelDataMap.TryGetValue(levelId, out LevelDataSO levelData))
        {
            if (levelData.blueprintReward > 0)
            {
                GlobalResourceManager.Instance?.AddResource(ResourceType.Blueprints, levelData.blueprintReward);
                Debug.Log($"[StrategyMapManager] ��������� {levelData.blueprintReward} �������� �� ������� {levelId}");
            }
        }
    }

    /// <summary>
    /// ��������� ����������� ���� ������� �� �����.
    /// ���������� ����� �������� ���������� ��� ���������� ������.
    /// ��������� ������ ������� �� ��������� ��������� ����� EventHub.
    /// </summary>
    private void UpdateLevelsAvailability()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[StrategyMapManager] SaveManager �� ������!");
            return;
        }

        int updatedLevels = 0;

        foreach (var levelData in campaignLevels)
        {
            if (levelData != null)
            {
                LevelState newState = levelData.GetLevelState(SaveManager.Instance);

                // ��������� �� ��������� ��������� ������
                EventHub.OnLevelStateChanged?.Invoke(levelData, newState);
                updatedLevels++;
            }
        }

        Debug.Log($"[StrategyMapManager] ��������� ��������� �������: {updatedLevels}");
    }

    /// <summary>
    /// �������� ������� �� �������������� �����.
    /// ��������� ������ ������� � ������ ����� EventHub.
    /// </summary>
    /// <param name="levelData">������ ���������� ������.</param>
    /// <returns>true - ���� ������� ������� ������, false - ���� ������� ����������.</returns>
    public bool SelectLevel(LevelDataSO levelData)
    {
        if (levelData == null)
        {
            Debug.LogWarning("[StrategyMapManager] ������� ������� null �������");
            return false;
        }

        // ��������� ����������� ������
        if (!levelData.IsLevelAvailable(SaveManager.Instance))
        {
            Debug.LogWarning($"[StrategyMapManager] ������� {levelData.levelId} ���������� ��� ������");
            return false;
        }

        selectedLevel = levelData;

        // ��������� � ������ ������ ����� EventHub
        EventHub.OnLevelSelected?.Invoke(levelData);

        Debug.Log($"[StrategyMapManager] ������ �������: {levelData.displayName}");

        return true;
    }

    /// <summary>
    /// ��������� ��������� �������.
    /// </summary>
    /// <returns>true - ���� ������� ������� �������, false - ���� ������� �� ������ ��� ����������.</returns>
    public bool LaunchSelectedLevel()
    {
        if (selectedLevel == null)
        {
            Debug.LogWarning("[StrategyMapManager] �� ������ ������� ��� �������");
            return false;
        }

        if (!selectedLevel.IsLevelAvailable(SaveManager.Instance))
        {
            Debug.LogWarning($"[StrategyMapManager] ��������� ������� {selectedLevel.levelId} ����������");
            return false;
        }

        Debug.Log($"[StrategyMapManager] ������ ������: {selectedLevel.levelId}");
        SceneLoaderManager.Instance.LoadTacticalLevel(selectedLevel.levelId);

        return true;
    }

    /// <summary>
    /// �������� ������ ������ �� ��������������.
    /// </summary>
    /// <param name="levelId">������������� ������.</param>
    /// <returns>������ ������ ��� null ���� �� ������.</returns>
    public LevelDataSO GetLevelData(string levelId)
    {
        levelDataMap.TryGetValue(levelId, out LevelDataSO levelData);
        return levelData;
    }

    /// <summary>
    /// �������� ��� ������ ��������.
    /// </summary>
    /// <returns>������ ���� ������� ��������.</returns>
    public LevelDataSO[] GetAllLevels()
    {
        return campaignLevels;
    }

    /// <summary>
    /// �������� ������� ��������� �������.
    /// </summary>
    /// <returns>��������� ������� ��� null ���� ������� �� ������.</returns>
    public LevelDataSO GetSelectedLevel()
    {
        return selectedLevel;
    }
}