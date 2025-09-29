using UnityEngine;

/// <summary>
/// Главный менеджер панели исследований, отвечающий за глобальное управление открытием, закрытием
/// и состоянием исследовательского интерфейса. Координирует работу между различными UI-компонентами
/// панели исследований и обеспечивает согласованность с глобальным состоянием игры.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является центральным координатором для всей исследовательской UI-системы.
/// Управляет видимостью панели, проверяет условия доступности и синхронизирует отображение глобальных ресурсов.</para>
/// <para><b>Взаимодействие:</b></para>
/// <list type="bullet">
/// <item><description>Взаимодействует с <see cref="GameStateManager"/> для проверки допустимости открытия панели</description></item>
/// <item><description>Координирует работу <see cref="UI_ResearchTreeController"/> для построения дерева технологий</description></item>
/// <item><description>Синхронизируется с <see cref="GlobalResourceManager"/> для отображения актуального количества чертежей</description></item>
/// <item><description>Оповещает другие системы через <see cref="EventHub"/> об изменениях состояния панели</description></item>
/// </list>
/// </remarks>
public class UI_ResearchPanelManager : MonoBehaviour
{
    /// <summary>
    /// Глобальная точка доступа к экземпляру менеджера панели исследований.
    /// Гарантирует единообразное управление исследовательским интерфейсом из любой точки игры.
    /// </summary>
    public static UI_ResearchPanelManager Instance { get; private set; }

    [Header("Research UI Components")]
    /// <summary>
    /// Ссылка на корневой GameObject панели исследований.
    /// Содержит все UI-элементы, связанные с интерфейсом исследований.
    /// </summary>
    [Tooltip("Корневой GameObject панели исследований.")]
    [SerializeField] private GameObject researchPanel;

    /// <summary>
    /// Ссылка на контроллер дерева исследований, отвечающий за визуальное построение
    /// и управление узлами технологий. Делегирует ему создание и обновление дерева.
    /// </summary>
    [Tooltip("Контроллер дерева исследований, управляющий узлами и ветками.")]
    [SerializeField] private UI_ResearchTreeController researchTreeController;

    /// <summary>
    /// Ссылка на UI-компонент отображения количества чертежей.
    /// Обеспечивает визуальную обратную связь о доступных глобальных ресурсах для исследований.
    /// </summary>
    [Tooltip("Компонент отображения количества чертежей (глобальный ресурс).")]
    [SerializeField] private UI_ResourceDisplay blueprintDisplay;

    /// <summary>
    /// Флаг, указывающий открыта ли в данный момент панель исследований.
    /// Используется для предотвращения конфликтов управления и синхронизации состояния UI.
    /// </summary>
    public bool IsResearchPanelOpen { get; private set; }

    /// <summary>
    /// Инициализирует singleton-экземпляр и настраивает сохранение между сценами.
    /// Убеждается, что глобальный UI исследований сохраняется при переходе между сценами.
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
    /// Инициализирует глобальный UI исследований при старте.
    /// Приводит панель в начальное закрытое состояние.
    /// </summary>
    private void Start()
    {
        InitializeResearchPanel();
    }

    /// <summary>
    /// Приводит панель исследований в начальное состояние.
    /// Гарантирует, что панель закрыта при старте игры или загрузке сцены.
    /// </summary>
    private void InitializeResearchPanel()
    {
        CloseResearchPanel();
        Debug.Log("[UI_ResearchPanelManager] Панель исследований инициализирована в закрытом состоянии");
    }

    /// <summary>
    /// Основной метод для переключения состояния панели исследований (открыть/закрыть).
    /// Выполняет проверки доступности и координирует все связанные системы.
    /// </summary>
    /// <example>
    /// Typical usage: Вызывается из UI кнопки "Исследования" на стратегической карте.
    /// <code>UI_ResearchPanelManager.Instance.ToggleResearchPanel();</code>
    /// </example>
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
    /// Выполняет проверки доступности, активирует панель и инициализирует дочерние компоненты.
    /// </summary>
    /// <remarks>
    /// <para>Условия открытия панели:</para>
    /// <list type="bullet">
    /// <item><description>Игра не находится в состоянии главного меню</description></item>
    /// <item><description>Панель еще не открыта</description></item>
    /// <item><description>Все необходимые ссылки инициализированы</description></item>
    /// </list>
    /// </remarks>
    public void OpenResearchPanel()
    {
        // Проверка наличия необходимых ссылок
        if (researchPanel == null)
        {
            Debug.LogError("[UI_ResearchPanelManager] Не назначена ссылка на researchPanel!");
            return;
        }

        // Защита от повторного открытия
        if (IsResearchPanelOpen)
        {
            Debug.LogWarning("[UI_ResearchPanelManager] Панель исследований уже открыта");
            return;
        }

        // Проверка доступности в текущем состоянии игры
        if (!CanOpenResearchPanel())
        {
            Debug.LogWarning($"[UI_ResearchPanelManager] Невозможно открыть панель исследований в текущем состоянии игры: {GameStateManager.Instance.CurrentState}");
            return;
        }

        // Активация панели и установка флага
        researchPanel.SetActive(true);
        IsResearchPanelOpen = true;

        // Инициализация дочерних компонентов
        InitializeResearchComponents();

        Debug.Log("[UI_ResearchPanelManager] Панель исследований успешно открыта");
    }

    /// <summary>
    /// Закрывает панель исследований и приводит все системы в согласованное состояние.
    /// Выполняет очистку временных данных и деактивацию UI-элементов.
    /// </summary>
    public void CloseResearchPanel()
    {
        // Проверка наличия необходимых ссылок
        if (researchPanel == null)
        {
            Debug.LogError("[UI_ResearchPanelManager] Не назначена ссылка на researchPanel!");
            return;
        }

        // Защита от повторного закрытия
        if (!IsResearchPanelOpen)
        {
            Debug.LogWarning("[UI_ResearchPanelManager] Панель исследований уже закрыта");
            return;
        }

        // Деактивация панели и сброс флага
        researchPanel.SetActive(false);
        IsResearchPanelOpen = false;

        // Очистка временных данных дочерних компонентов
        CleanupResearchComponents();

        Debug.Log("[UI_ResearchPanelManager] Панель исследований успешно закрыта");
    }

    /// <summary>
    /// Проверяет, можно ли открыть панель исследований в текущем состоянии игры.
    /// Определяет бизнес-логику доступности исследовательского интерфейса.
    /// </summary>
    /// <returns>true - если панель можно открыть, false - в противном случае.</returns>
    private bool CanOpenResearchPanel()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("[UI_ResearchPanelManager] GameStateManager не найден!");
            return false;
        }

        var currentState = GameStateManager.Instance.CurrentState;

        // Панель исследований недоступна в главном меню и на паузе
        return currentState != GameState.MainMenu && currentState != GameState.Paused;
    }

    /// <summary>
    /// Инициализирует все дочерние компоненты панели исследований при открытии.
    /// Координирует последовательную инициализацию зависимых систем.
    /// </summary>
    private void InitializeResearchComponents()
    {
        // Инициализация дерева исследований
        if (researchTreeController != null)
        {
            researchTreeController.InitializeResearchTree();
        }
        else
        {
            Debug.LogWarning("[UI_ResearchPanelManager] ResearchTreeController не назначен!");
        }

        // Обновление отображения ресурсов
        UpdateBlueprintDisplay();
    }

    /// <summary>
    /// Выполняет очистку всех дочерних компонентов панели исследований при закрытии.
    /// Освобождает временные ресурсы и сбрасывает состояние.
    /// </summary>
    private void CleanupResearchComponents()
    {
        // Очистка дерева исследований
        if (researchTreeController != null)
        {
            researchTreeController.CleanupResearchTree();
        }
    }

    /// <summary>
    /// Обновляет отображение количества чертежей в UI.
    /// Запрашивает актуальные данные у GlobalResourceManager и синхронизирует визуальное представление.
    /// </summary>
    public void UpdateBlueprintDisplay()
    {
        if (blueprintDisplay == null)
        {
            Debug.LogWarning("[UI_ResearchPanelManager] BlueprintDisplay не назначен!");
            return;
        }

        if (GlobalResourceManager.Instance == null)
        {
            Debug.LogError("[UI_ResearchPanelManager] GlobalResourceManager не найден!");
            return;
        }

        int blueprints = GlobalResourceManager.Instance.GetResourceAmount(ResourceType.Blueprints);
        blueprintDisplay.UpdateResourceDisplay(ResourceType.Blueprints, blueprints);

        Debug.Log($"[UI_ResearchPanelManager] Отображение чертежей обновлено: {blueprints}");
    }

    /// <summary>
    /// Подписывается на события изменения глобальных ресурсов при активации объекта.
    /// Обеспечивает реактивное обновление UI при изменении количества чертежей.
    /// </summary>
    private void OnEnable()
    {
        EventHub.OnGlobalResourceChanged += HandleGlobalResourceChanged;
        Debug.Log("[UI_ResearchPanelManager] Подписка на события глобальных ресурсов активирована");
    }

    /// <summary>
    /// Отписывается от событий при деактивации объекта для предотвращения утечек памяти.
    /// Гарантирует корректное освобождение ресурсов при уничтожении объекта.
    /// </summary>
    private void OnDisable()
    {
        EventHub.OnGlobalResourceChanged -= HandleGlobalResourceChanged;
        Debug.Log("[UI_ResearchPanelManager] Подписка на события глобальных ресурсов деактивирована");
    }

    /// <summary>
    /// Обработчик события изменения глобальных ресурсов.
    /// Автоматически обновляет отображение чертежей при изменении их количества.
    /// </summary>
    /// <param name="resourceType">Тип изменившегося ресурса.</param>
    /// <param name="newAmount">Новое количество ресурса.</param>
    private void HandleGlobalResourceChanged(ResourceType resourceType, int newAmount)
    {
        // Реагируем только на изменения чертежей и только когда панель открыта
        if (resourceType == ResourceType.Blueprints && IsResearchPanelOpen)
        {
            UpdateBlueprintDisplay();
            Debug.Log($"[UI_ResearchPanelManager] Обнаружено изменение чертежей: {newAmount}");
        }
    }
}