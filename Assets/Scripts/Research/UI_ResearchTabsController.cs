using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Контроллер управления вкладками интерфейса исследований.
/// Обрабатывает переключение между разделами исследований (Башни, Инженерия, Способности).
/// Управляет визуальным состоянием кнопок вкладок и их доступностью в зависимости от прогресса игрока.
/// </summary>
public class UI_ResearchTabsController : MonoBehaviour
{
    [Header("Tab Buttons")]
    /// <summary>
    /// Кнопка вкладки исследований башен.
    /// Доступна с начала игры.
    /// </summary>
    [Tooltip("Кнопка вкладки 'Башни'.")]
    [SerializeField] private Button towersTabButton;

    /// <summary>
    /// Кнопка вкладки инженерных исследований.
    /// Разблокируется после прохождения определенного уровня.
    /// </summary>
    [Tooltip("Кнопка вкладки 'Инженерия'.")]
    [SerializeField] private Button engineeringTabButton;

    /// <summary>
    /// Кнопка вкладки исследований способностей.
    /// Разблокируется после прохождения определенного уровня.
    /// </summary>
    [Tooltip("Кнопка вкладки 'Способности'.")]
    [SerializeField] private Button abilitiesTabButton;

    [Header("Tab Icons")]
    /// <summary>
    /// Иконка вкладки башен.
    /// </summary>
    [Tooltip("Иконка вкладки 'Башни'.")]
    [SerializeField] private Image towersTabIcon;

    /// <summary>
    /// Иконка вкладки инженерии.
    /// </summary>
    [Tooltip("Иконка вкладки 'Инженерия'.")]
    [SerializeField] private Image engineeringTabIcon;

    /// <summary>
    /// Иконка вкладки способностей.
    /// </summary>
    [Tooltip("Иконка вкладки 'Способности'.")]
    [SerializeField] private Image abilitiesTabIcon;

    [Header("Tab Texts")]
    /// <summary>
    /// Текст вкладки башен.
    /// </summary>
    [Tooltip("Текст вкладки 'Башни'.")]
    [SerializeField] private TextMeshProUGUI towersTabText;

    /// <summary>
    /// Текст вкладки инженерии.
    /// </summary>
    [Tooltip("Текст вкладки 'Инженерия'.")]
    [SerializeField] private TextMeshProUGUI engineeringTabText;

    /// <summary>
    /// Текст вкладки способностей.
    /// </summary>
    [Tooltip("Текст вкладки 'Способности'.")]
    [SerializeField] private TextMeshProUGUI abilitiesTabText;

    [Header("Colors")]
    /// <summary>
    /// Цвет активной вкладки.
    /// Требует замены на другой UI элемент
    /// </summary>
    [Tooltip("Цвет активной вкладки.")]
    [SerializeField] private Color activeTabColor = Color.white;

    /// <summary>
    /// Цвет неактивной вкладки.
    /// Требует замены на другой UI элемент
    /// </summary>
    [Tooltip("Цвет неактивной вкладки.")]
    [SerializeField] private Color inactiveTabColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    /// <summary>
    /// Цвет заблокированной вкладки (иконка).
    /// Требует замены на другой UI элемент
    /// </summary>
    [Tooltip("Цвет заблокированной вкладки (иконка).")]
    [SerializeField] private Color lockedTabColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    /// <summary>
    /// Цвет текста заблокированной вкладки.
    /// Требует замены на другой UI элемент
    /// </summary>
    [Tooltip("Цвет текста заблокированной вкладки.")]
    [SerializeField] private Color lockedTextColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);

    /// <summary>
    /// Текущая активная вкладка.
    /// </summary>
    private ResearchTabType currentTab = ResearchTabType.Towers;

    /// <summary>
    /// Инициализирует контроллер вкладок при старте.
    /// Подписывается на события кнопок и устанавливает начальное состояние.
    /// </summary>
    private void Start()
    {
        // Подписываемся на кнопки табов
        towersTabButton.onClick.AddListener(() => SwitchTab(ResearchTabType.Towers));
        engineeringTabButton.onClick.AddListener(() => SwitchTab(ResearchTabType.Engineering));
        abilitiesTabButton.onClick.AddListener(() => SwitchTab(ResearchTabType.Abilities));

        // Инициализируем табы
        UpdateTabsAvailability();
        SwitchTab(ResearchTabType.Towers);
    }

    /// <summary>
    /// Переключает активную вкладку исследований.
    /// Проверяет доступность вкладки перед переключением.
    /// Оповещает систему о смене вкладки через EventHub.
    /// </summary>
    /// <param name="tabType">Тип вкладки для переключения.</param>
    private void SwitchTab(ResearchTabType tabType)
    {
        if (!ResearchManager.Instance.IsTabAvailable(tabType))
        {
            Debug.Log($"Tab {tabType} is locked!");
            return;
        }

        currentTab = tabType;
        UpdateTabsVisuals();

        // Оповещаем панель исследований о смене таба
        EventHub.OnResearchTabChanged?.Invoke(tabType);
    }

    /// <summary>
    /// Обновляет доступность вкладок на основе прогресса игрока.
    /// Проверяет через ResearchManager, какие вкладки должны быть доступны.
    /// Обновляет интерактивность кнопок и визуал заблокированных вкладок.
    /// </summary>
    private void UpdateTabsAvailability()
    {
        // Обновляем доступность табов
        bool engineeringAvailable = ResearchManager.Instance.IsTabAvailable(ResearchTabType.Engineering);
        bool abilitiesAvailable = ResearchManager.Instance.IsTabAvailable(ResearchTabType.Abilities);

        engineeringTabButton.interactable = engineeringAvailable;
        abilitiesTabButton.interactable = abilitiesAvailable;

        // Обновляем визуал табов
        if (!engineeringAvailable)
        {
            engineeringTabIcon.color = lockedTabColor;
            engineeringTabText.color = lockedTextColor;
        }

        if (!abilitiesAvailable)
        {
            abilitiesTabIcon.color = lockedTabColor;
            abilitiesTabText.color = lockedTextColor;
        }
    }

    /// <summary>
    /// Обновляет визуальное представление всех вкладок.
    /// Устанавливает правильные цвета для активной и неактивных вкладок.
    /// </summary>
    private void UpdateTabsVisuals()
    {
        // Сбрасываем все цвета к неактивным
        ResetAllTabsToInactive();

        // Устанавливаем цвет активного таба
        switch (currentTab)
        {
            case ResearchTabType.Towers:
                SetTabActive(towersTabIcon, towersTabText);
                break;
            case ResearchTabType.Engineering:
                SetTabActive(engineeringTabIcon, engineeringTabText);
                break;
            case ResearchTabType.Abilities:
                SetTabActive(abilitiesTabIcon, abilitiesTabText);
                break;
        }
    }

    /// <summary>
    /// Сбрасывает визуальное состояние всех вкладок к неактивному.
    /// Восстанавливает цвета заблокированных вкладок после сброса.
    /// </summary>
    private void ResetAllTabsToInactive()
    {
        towersTabIcon.color = inactiveTabColor;
        towersTabText.color = inactiveTabColor;

        engineeringTabIcon.color = inactiveTabColor;
        engineeringTabText.color = inactiveTabColor;

        abilitiesTabIcon.color = inactiveTabColor;
        abilitiesTabText.color = inactiveTabColor;

        // Восстанавливаем цвета заблокированных табов
        UpdateTabsAvailability();
    }

    /// <summary>
    /// Устанавливает визуальное состояние для активной вкладки.
    /// </summary>
    /// <param name="tabIcon">Иконка вкладки.</param>
    /// <param name="tabText">Текст вкладки.</param>
    private void SetTabActive(Image tabIcon, TextMeshProUGUI tabText)
    {
        tabIcon.color = activeTabColor;
        tabText.color = activeTabColor;
    }

    /// <summary>
    /// Метод для принудительной разблокировки вкладки.
    /// Вызывается при выполнении условий разблокировки (например, прохождение уровня).
    /// </summary>
    /// <param name="tabType">Тип вкладки для разблокировки.</param>
    public void UnlockTab(ResearchTabType tabType)
    {
        ResearchTabSO tab = System.Array.Find(ResearchManager.Instance.GetResearchTabs(), t => t.TabType == tabType);
        if (tab != null)
        {
            tab.IsLocked = false;
            UpdateTabsAvailability();
            UpdateTabsVisuals();
        }
    }
}