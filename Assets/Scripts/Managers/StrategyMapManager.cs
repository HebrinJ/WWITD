using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Менеджер стратегической карты, отвечающий за управление уровнями кампании.
/// Координирует отображение уровней, их доступность и прогресс игрока.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Центральный координатор для всей логики стратегической карты.
/// Управляет состоянием уровней, обрабатывает выбор игрока и синхронизируется с системой сохранения.</para>
/// <para><b>Взаимодействие через EventHub:</b> Все уведомления о изменениях отправляются через EventHub,
/// что обеспечивает слабую связность с UI и другими системами.</para>
/// </remarks>
public class StrategyMapManager : MonoBehaviour
{
    /// <summary>
    /// Глобальная точка доступа к экземпляру менеджера стратегической карты.
    /// </summary>
    public static StrategyMapManager Instance { get; private set; }

    /// <summary>
    /// Массив всех уровней кампании в правильном порядке.
    /// Должен быть назначен в инспекторе Unity.
    /// </summary>
    [Tooltip("Массив всех уровней кампании. Упорядочьте в правильной последовательности.")]
    [SerializeField] private LevelDataSO[] campaignLevels;

    /// <summary>
    /// Словарь для быстрого доступа к данным уровня по идентификатору.
    /// </summary>
    private Dictionary<string, LevelDataSO> levelDataMap = new Dictionary<string, LevelDataSO>();

    /// <summary>
    /// Текущий выбранный игроком уровень.
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
    /// Инициализирует singleton-экземпляр менеджера.
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
        Debug.Log("[StrategyMapManager] Singleton инициализирован");
    }

    /// <summary>
    /// Инициализирует данные уровней и создает словарь для быстрого доступа.
    /// </summary>
    private void InitializeLevelData()
    {
        levelDataMap.Clear();

        if (campaignLevels == null || campaignLevels.Length == 0)
        {
            Debug.LogError("[StrategyMapManager] Не назначены уровни кампании!");
            return;
        }

        foreach (var levelData in campaignLevels)
        {
            if (levelData != null && !string.IsNullOrEmpty(levelData.levelId))
            {
                levelDataMap[levelData.levelId] = levelData;
            }
        }

        Debug.Log($"[StrategyMapManager] Инициализировано уровней: {levelDataMap.Count}");
    }

    /// <summary>
    /// Обработчик загрузки данных сохранения.
    /// Обновляет состояние уровней после загрузки прогресса игрока.
    /// </summary>
    /// <param name="playerData">Загруженные данные игрока.</param>
    private void HandleSaveDataLoaded(PlayerSaveData playerData)
    {
        UpdateLevelsAvailability();
        Debug.Log("[StrategyMapManager] Состояние уровней обновлено после загрузки сохранения");
    }

    /// <summary>
    /// Обработчик завершения уровня.
    /// Открывает следующие уровни и обновляет состояние карты.
    /// </summary>
    /// <param name="levelId">Идентификатор завершенного уровня.</param>
    private void HandleLevelCompleted(string levelId)
    {
        // Начисляем награду за уровень
        AwardLevelCompletion(levelId);

        // Обновляем доступность уровней
        UpdateLevelsAvailability();

        Debug.Log($"[StrategyMapManager] Уровень {levelId} завершен. Доступность уровней обновлена");
    }

    /// <summary>
    /// Начисляет награду за завершение уровня.
    /// </summary>
    /// <param name="levelId">Идентификатор завершенного уровня.</param>
    private void AwardLevelCompletion(string levelId)
    {
        if (levelDataMap.TryGetValue(levelId, out LevelDataSO levelData))
        {
            if (levelData.blueprintReward > 0)
            {
                GlobalResourceManager.Instance?.AddResource(ResourceType.Blueprints, levelData.blueprintReward);
                Debug.Log($"[StrategyMapManager] Начислено {levelData.blueprintReward} чертежей за уровень {levelId}");
            }
        }
    }

    /// <summary>
    /// Обновляет доступность всех уровней на карте.
    /// Вызывается после загрузки сохранения или завершения уровня.
    /// Оповещает другие системы об изменении состояний через EventHub.
    /// </summary>
    private void UpdateLevelsAvailability()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[StrategyMapManager] SaveManager не найден!");
            return;
        }

        int updatedLevels = 0;

        foreach (var levelData in campaignLevels)
        {
            if (levelData != null)
            {
                LevelState newState = levelData.GetLevelState(SaveManager.Instance);

                // Оповещаем об изменении состояния уровня
                EventHub.OnLevelStateChanged?.Invoke(levelData, newState);
                updatedLevels++;
            }
        }

        Debug.Log($"[StrategyMapManager] Обновлено состояний уровней: {updatedLevels}");
    }

    /// <summary>
    /// Выбирает уровень на стратегической карте.
    /// Оповещает другие системы о выборе через EventHub.
    /// </summary>
    /// <param name="levelData">Данные выбранного уровня.</param>
    /// <returns>true - если уровень успешно выбран, false - если уровень недоступен.</returns>
    public bool SelectLevel(LevelDataSO levelData)
    {
        if (levelData == null)
        {
            Debug.LogWarning("[StrategyMapManager] Попытка выбрать null уровень");
            return false;
        }

        // Проверяем доступность уровня
        if (!levelData.IsLevelAvailable(SaveManager.Instance))
        {
            Debug.LogWarning($"[StrategyMapManager] Уровень {levelData.levelId} недоступен для выбора");
            return false;
        }

        selectedLevel = levelData;

        // Оповещаем о выборе уровня через EventHub
        EventHub.OnLevelSelected?.Invoke(levelData);

        Debug.Log($"[StrategyMapManager] Выбран уровень: {levelData.displayName}");

        return true;
    }

    /// <summary>
    /// Запускает выбранный уровень.
    /// </summary>
    /// <returns>true - если уровень успешно запущен, false - если уровень не выбран или недоступен.</returns>
    public bool LaunchSelectedLevel()
    {
        if (selectedLevel == null)
        {
            Debug.LogWarning("[StrategyMapManager] Не выбран уровень для запуска");
            return false;
        }

        if (!selectedLevel.IsLevelAvailable(SaveManager.Instance))
        {
            Debug.LogWarning($"[StrategyMapManager] Выбранный уровень {selectedLevel.levelId} недоступен");
            return false;
        }

        Debug.Log($"[StrategyMapManager] Запуск уровня: {selectedLevel.levelId}");
        SceneLoaderManager.Instance.LoadTacticalLevel(selectedLevel.levelId);

        return true;
    }

    /// <summary>
    /// Получает данные уровня по идентификатору.
    /// </summary>
    /// <param name="levelId">Идентификатор уровня.</param>
    /// <returns>Данные уровня или null если не найден.</returns>
    public LevelDataSO GetLevelData(string levelId)
    {
        levelDataMap.TryGetValue(levelId, out LevelDataSO levelData);
        return levelData;
    }

    /// <summary>
    /// Получает все уровни кампании.
    /// </summary>
    /// <returns>Массив всех уровней кампании.</returns>
    public LevelDataSO[] GetAllLevels()
    {
        return campaignLevels;
    }

    /// <summary>
    /// Получает текущий выбранный уровень.
    /// </summary>
    /// <returns>Выбранный уровень или null если уровень не выбран.</returns>
    public LevelDataSO GetSelectedLevel()
    {
        return selectedLevel;
    }
}