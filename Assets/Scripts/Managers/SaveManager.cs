using UnityEngine;
using System;

/// <summary>
/// Центральный менеджер сохранения и загрузки игрового прогресса.
/// Реализует шаблон Singleton и сохраняется между сценами (DontDestroyOnLoad).
/// Координирует сохранение состояния всех систем игры (прогресс уровней, глобальные ресурсы, исследования).
/// Автоматически сохраняет данные при выходе из игры и приостановке приложения.
/// </summary>
/// <remarks>
/// <para><b>Архитектура сохранения:</b></para>
/// <list type="bullet">
/// <item><description>Использует PlayerPrefs для хранения JSON-сериализованных данных</description></item>
/// <item><description>Поддерживает миграцию данных между версиями</description></item>
/// <item><description>Обеспечивает атомарность операций сохранения</description></item>
/// <item><description>Интегрируется с EventHub для согласованной работы с другими системами</description></item>
/// </list>
/// </remarks>
public class SaveManager : MonoBehaviour
{
    /// <summary>
    /// Глобальная точка доступа к экземпляру SaveManager.
    /// Гарантирует, что в игре существует только один экземпляр менеджера сохранений.
    /// </summary>
    public static SaveManager Instance { get; private set; }

    /// <summary>
    /// Ключ для хранения данных игрока в PlayerPrefs.
    /// </summary>
    private const string PLAYER_DATA_KEY = "PlayerSaveData";

    /// <summary>
    /// Текущая версия системы сохранения.
    /// Используется для миграции данных при обновлениях.
    /// </summary>
    private const string CURRENT_SAVE_VERSION = "1.0.0";

    /// <summary>
    /// Ссылка на ScriptableObject, содержащий состояние исследований игрока.
    /// Управляет сохранением и загрузкой прогресса в древе технологий.
    /// Должен быть назначен в инспекторе Unity.
    /// </summary>
    [Tooltip("ScriptableObject для сохранения состояния исследований. Назначьте в инспекторе.")]
    [SerializeField] private ResearchStateSO researchState;

    /// <summary>
    /// Текущие данные сохранения игрока.
    /// Загружаются при старте и сохраняются при изменении.
    /// </summary>
    private PlayerSaveData playerData;

    private void Awake()
    {
        InitializeSingleton();
    }

    /// <summary>
    /// Инициализирует singleton-экземпляр SaveManager.
    /// Гарантирует единственность экземпляра и сохранение между сценами.
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
        Debug.Log("[SaveManager] Singleton инициализирован");
    }

    /// <summary>
    /// Подписывается на события при включении менеджера.
    /// Обеспечивает реактивное сохранение данных при изменениях в игре.
    /// </summary>
    private void OnEnable()
    {
        EventHub.OnLevelCompleted += HandleLevelCompleted;
        EventHub.OnGlobalResourcesSaveRequested += HandleGlobalResourcesSaveRequested;

        // Загружаем данные после подписки на события
        LoadAllData();
    }

    /// <summary>
    /// Отписывается от событий при выключении менеджера.
    /// </summary>
    private void OnDisable()
    {
        EventHub.OnLevelCompleted -= HandleLevelCompleted;
        EventHub.OnGlobalResourcesSaveRequested -= HandleGlobalResourcesSaveRequested;
    }

    /// <summary>
    /// Автоматически сохраняет все данные при завершении работы приложения.
    /// Вызывается системой Unity при закрытии игры.
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveAllData();
    }

    /// <summary>
    /// Автоматически сохраняет все данные при приостановке приложения.
    /// Особенно важно для мобильных платформ, когда игрок сворачивает приложение.
    /// </summary>
    /// <param name="pauseStatus">True если приложение приостановлено, false если возобновлено.</param>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllData();
        }
    }

    /// <summary>
    /// Обработчик события завершения уровня.
    /// Сохраняет прогресс прохождения уровня.
    /// </summary>
    /// <param name="levelId">Идентификатор завершенного уровня.</param>
    private void HandleLevelCompleted(string levelId)
    {
        CompleteLevel(levelId);
    }

    /// <summary>
    /// Обработчик запроса на сохранение глобальных ресурсов.
    /// Сохраняет текущее состояние глобальных ресурсов.
    /// </summary>
    private void HandleGlobalResourcesSaveRequested()
    {
        SaveGlobalResources();
    }

    /// <summary>
    /// Сохраняет все игровые данные в постоянное хранилище.
    /// Вызывает методы сохранения для всех подсистем (прогресс, ресурсы, исследования).
    /// Принудительно записывает данные в PlayerPrefs.
    /// </summary>
    public void SaveAllData()
    {
        if (playerData == null)
        {
            Debug.LogWarning("[SaveManager] Нет данных для сохранения");
            return;
        }

        try
        {
            // Обновляем время сохранения
            playerData.UpdateSaveTime();

            // Сохраняем данные игрока
            SavePlayerData();

            // Сохраняем состояние исследований
            SaveResearchState();

            // Принудительно записываем данные
            PlayerPrefs.Save();

            Debug.Log($"[SaveManager] Данные сохранены. Время: {playerData.lastSaveTime}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Ошибка при сохранении данных: {e.Message}");
        }
    }

    /// <summary>
    /// Загружает все игровые данные из постоянного хранилища.
    /// Восстанавливает состояние всех подсистем игры.
    /// </summary>
    public void LoadAllData()
    {
        try
        {
            // Загружаем данные игрока
            LoadPlayerData();

            // Загружаем состояние исследований
            LoadResearchState();

            // Синхронизируем глобальные ресурсы
            SyncGlobalResources();

            // Оповещаем системы о загрузке данных через EventHub
            EventHub.OnSaveDataLoaded?.Invoke(playerData);

            Debug.Log($"[SaveManager] Данные загружены. Пройдено уровней: {playerData.completedLevels.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Ошибка при загрузке данных: {e.Message}");
        }
    }

    /// <summary>
    /// Сохраняет данные игрока в PlayerPrefs в формате JSON.
    /// </summary>
    private void SavePlayerData()
    {
        string jsonData = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(PLAYER_DATA_KEY, jsonData);
        Debug.Log($"[SaveManager] Данные игрока сохранены. JSON длина: {jsonData.Length}");
    }

    /// <summary>
    /// Загружает данные игрока из PlayerPrefs.
    /// Создает новые данные, если сохранение не найдено.
    /// </summary>
    private void LoadPlayerData()
    {
        if (PlayerPrefs.HasKey(PLAYER_DATA_KEY))
        {
            string jsonData = PlayerPrefs.GetString(PLAYER_DATA_KEY);
            playerData = JsonUtility.FromJson<PlayerSaveData>(jsonData);

            // Проверяем версию сохранения и выполняем миграцию при необходимости
            MigrateSaveDataIfNeeded();

            Debug.Log($"[SaveManager] Данные игрока загружены из сохранения");
        }
        else
        {
            playerData = new PlayerSaveData();
            Debug.Log("[SaveManager] Созданы новые данные игрока");
        }
    }

    /// <summary>
    /// Выполняет миграцию данных сохранения при изменении версии.
    /// Обеспечивает совместимость с предыдущими версиями игры.
    /// </summary>
    private void MigrateSaveDataIfNeeded()
    {
        if (playerData.saveVersion != CURRENT_SAVE_VERSION)
        {
            Debug.Log($"[SaveManager] Миграция данных с версии {playerData.saveVersion} на {CURRENT_SAVE_VERSION}");

            // Здесь можно добавить логику миграции для будущих версий
            playerData.saveVersion = CURRENT_SAVE_VERSION;

            Debug.Log("[SaveManager] Миграция данных завершена");
        }
    }

    /// <summary>
    /// Сохраняет состояние исследований через связанный ResearchStateSO.
    /// </summary>
    private void SaveResearchState()
    {
        if (researchState != null)
        {
            researchState.SaveState();
            Debug.Log("[SaveManager] Состояние исследований сохранено");
        }
        else
        {
            Debug.LogWarning("[SaveManager] ResearchStateSO не назначен!");
        }
    }

    /// <summary>
    /// Загружает состояние исследований через связанный ResearchStateSO.
    /// </summary>
    private void LoadResearchState()
    {
        if (researchState != null)
        {
            researchState.LoadState();
            Debug.Log("[SaveManager] Состояние исследований загружено");
        }
        else
        {
            Debug.LogWarning("[SaveManager] ResearchStateSO не назначен!");
        }
    }

    /// <summary>
    /// Синхронизирует глобальные ресурсы между SaveManager и GlobalResourceManager.
    /// Обеспечивает согласованность данных после загрузки.
    /// </summary>
    private void SyncGlobalResources()
    {
        if (GlobalResourceManager.Instance == null)
        {
            Debug.LogWarning("[SaveManager] GlobalResourceManager не найден для синхронизации");
            return;
        }

        // Синхронизируем чертежи
        int blueprints = playerData.GetGlobalResource(ResourceType.Blueprints);
        GlobalResourceManager.Instance.AddResource(ResourceType.Blueprints, blueprints);

        Debug.Log($"[SaveManager] Глобальные ресурсы синхронизированы. Чертежи: {blueprints}");
    }

    /// <summary>
    /// Отмечает уровень как пройденный и сохраняет прогресс.
    /// </summary>
    /// <param name="levelId">Идентификатор уровня (например, "Level01").</param>
    public void CompleteLevel(string levelId)
    {
        if (playerData.AddCompletedLevel(levelId))
        {
            Debug.Log($"[SaveManager] Уровень {levelId} отмечен как пройденный");

            SaveAllData();
        }
        else
        {
            Debug.Log($"[SaveManager] Уровень {levelId} уже был пройден ранее");
        }
    }

    /// <summary>
    /// Проверяет, пройден ли указанный уровень.
    /// </summary>
    /// <param name="levelId">Идентификатор уровня для проверки.</param>
    /// <returns>true - если уровень пройден, false - в противном случае.</returns>
    public bool IsLevelCompleted(string levelId)
    {
        return playerData.IsLevelCompleted(levelId);
    }

    /// <summary>
    /// Сохраняет текущее состояние глобальных ресурсов.
    /// </summary>
    public void SaveGlobalResources()
    {
        if (GlobalResourceManager.Instance == null)
        {
            Debug.LogWarning("[SaveManager] GlobalResourceManager не найден для сохранения");
            return;
        }

        // Сохраняем все типы глобальных ресурсов
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            int amount = GlobalResourceManager.Instance.GetResourceAmount(resourceType);
            playerData.SetGlobalResource(resourceType, amount);
        }

        Debug.Log("[SaveManager] Глобальные ресурсы сохранены");
    }

    /// <summary>
    /// Получает текущие данные сохранения игрока (только для чтения).
    /// </summary>
    /// <returns>Текущие данные сохранения.</returns>
    public PlayerSaveData GetPlayerData()
    {
        return playerData;
    }

    /// <summary>
    /// Сбрасывает весь игровой прогресс (только для тестирования).
    /// Удаляет все сохраненные данные и сбрасывает состояние исследований.
    /// ВНИМАНИЕ: Этот метод должен быть удален или защищен в production-версии.
    /// </summary>
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();

        if (researchState != null)
        {
            researchState.ResetState();
        }

        playerData = new PlayerSaveData();

        Debug.Log("[SaveManager] Весь прогресс сброшен");
    }
}