using UnityEngine;

/// <summary>
/// Главный менеджер пользовательского интерфейса игры.
/// Управляет глобальными UI-окнами, которые сохраняются между сценами (DontDestroyOnLoad).
/// Координирует отображение исследовательской панели, ресурсов и других глобальных элементов UI.
/// ТРЕБУЕТ ПЕРЕСМОТРА АРХИТЕКТУРЫ: в текущей реализации нарушает принцип единственной ответственности,
/// объединяя управление разными несвязанными UI-компонентами.
/// </summary>
public class UI_Manager : MonoBehaviour
{
    /// <summary>
    /// Глобальная точка доступа к экземпляру UI_Manager.
    /// Гарантирует, что в игре существует только один экземпляр менеджера интерфейса.
    /// </summary>
    public static UI_Manager Instance { get; private set; }

    [Header("Global UI Windows")]
    /// <summary>
    /// Ссылка на GameObject панели исследований (древа технологий).
    /// Должна быть назначена в инспекторе Unity.
    /// </summary>
    [Tooltip("Панель интерфейса исследований (древа технологий).")]
    [SerializeField] private GameObject researchPanel;

    [Header("Resource Displays")]
    /// <summary>
    /// Ссылка на UI-компонент отображения количества чертежей.
    /// Отображает глобальный ресурс Blueprints в интерфейсе исследований.
    /// </summary>
    [Tooltip("Компонент отображения количества чертежей (глобальный ресурс).")]
    [SerializeField] private UI_ResourceDisplay blueprintDisplay;

    /// <summary>
    /// Флаг, указывающий открыта ли в данный момент панель исследований.
    /// Используется для предотвращения множественного открытия и управления состоянием UI.
    /// </summary>
    public bool IsResearchPanelOpen { get; private set; }

    /// <summary>
    /// Инициализирует singleton-экземпляр и настраивает сохранение между сценами.
    /// Убеждается, что глобальный UI сохраняется при переходе между сценами.
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
    }

    /// <summary>
    /// Инициализирует глобальный UI при старте.
    /// Закрывает все глобальные панели и приводит UI в начальное состояние.
    /// </summary>
    private void Start()
    {
        InitializeGlobalUI();
    }

    /// <summary>
    /// Приводит глобальный UI в начальное состояние.
    /// Закрывает все открытые глобальные панели (на данный момент только исследовательскую).
    /// </summary>
    private void InitializeGlobalUI()
    {
        CloseResearchPanel();
    }

    /// <summary>
    /// Переключает состояние панели исследований (открыть/закрыть).
    /// Основной метод для управления видимостью исследовательского интерфейса.
    /// </summary>
    public void ToggleResearchPanel()
    {
        if (IsResearchPanelOpen)
        {
            CloseResearchPanel();
        }
        else
        {
            OpenResearchPanel();
        }
    }

    /// <summary>
    /// Открывает панель исследований, если это возможно в текущем состоянии игры.
    /// Проверяет доступность исследований в текущем GameState.
    /// Обновляет отображение чертежей при открытии.
    /// </summary>
    public void OpenResearchPanel()
    {
        if (researchPanel == null) return;
        if (IsResearchPanelOpen) return;

        // Проверяем, можно ли открывать панель в текущем состоянии игры
        if (GameStateManager.Instance.CurrentState == GameState.MainMenu)
        {
            Debug.LogWarning("Cannot open research panel in Main Menu state");
            return;
        }

        researchPanel.SetActive(true);
        IsResearchPanelOpen = true;

        UpdateBlueprintDisplay();
        Debug.Log("Research panel opened (without pausing game)");
    }

    /// <summary>
    /// Закрывает панель исследований.
    /// Сбрасывает флаг состояния и деактивирует GameObject панели.
    /// </summary>
    public void CloseResearchPanel()
    {
        if (researchPanel == null) return;
        if (!IsResearchPanelOpen) return;

        researchPanel.SetActive(false);
        IsResearchPanelOpen = false;

        Debug.Log("Research panel closed");
    }

    /// <summary>
    /// Обновляет отображение количества чертежей в UI.
    /// Запрашивает актуальное значение у GlobalResourceManager и передает в UI_ResourceDisplay.
    /// </summary>
    public void UpdateBlueprintDisplay()
    {
        if (blueprintDisplay != null && GlobalResourceManager.Instance != null)
        {
            int blueprints = GlobalResourceManager.Instance.GetResourceAmount(ResourceType.Blueprints);
            blueprintDisplay.UpdateResourceDisplay(ResourceType.Blueprints, blueprints);
        }
    }

    /// <summary>
    /// Подписывается на события изменения глобальных ресурсов при активации объекта.
    /// </summary>
    private void OnEnable()
    {
        EventHub.OnGlobalResourceChanged += HandleGlobalResourceChanged;
    }

    /// <summary>
    /// Отписывается от событий при деактивации объекта для предотвращения утечек памяти.
    /// </summary>
    private void OnDisable()
    {
        EventHub.OnGlobalResourceChanged -= HandleGlobalResourceChanged;
    }

    /// <summary>
    /// Обработчик изменения глобальных ресурсов.
    /// Обновляет отображение чертежей, если панель исследований открыта и изменились чертежи.
    /// </summary>
    /// <param name="type">Тип изменившегося ресурса.</param>
    /// <param name="amount">Новое количество ресурса.</param>
    private void HandleGlobalResourceChanged(ResourceType type, int amount)
    {
        if (type == ResourceType.Blueprints && IsResearchPanelOpen)
        {
            UpdateBlueprintDisplay();
        }
    }
}