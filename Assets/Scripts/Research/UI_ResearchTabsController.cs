using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ResearchTabsController : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private Button towersTabButton;
    [SerializeField] private Button engineeringTabButton;
    [SerializeField] private Button abilitiesTabButton;

    [Header("Tab Icons")]
    [SerializeField] private Image towersTabIcon;
    [SerializeField] private Image engineeringTabIcon;
    [SerializeField] private Image abilitiesTabIcon;

    [Header("Tab Texts")]
    [SerializeField] private TextMeshProUGUI towersTabText;
    [SerializeField] private TextMeshProUGUI engineeringTabText;
    [SerializeField] private TextMeshProUGUI abilitiesTabText;

    [Header("Colors")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color lockedTabColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color lockedTextColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);

    private ResearchTabType currentTab = ResearchTabType.Towers;

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

    private void UpdateTabsAvailability()
    {
        // Обновляем доступность табов
        bool engineeringAvailable = ResearchManager.Instance.IsTabAvailable(ResearchTabType.Engineering);
        bool abilitiesAvailable = ResearchManager.Instance.IsTabAvailable(ResearchTabType.Abilities);

        engineeringTabButton.interactable = engineeringAvailable;
        abilitiesTabButton.interactable = abilitiesAvailable;

        // Обновляем визуал заблокированных табов
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

    private void SetTabActive(Image tabIcon, TextMeshProUGUI tabText)
    {
        tabIcon.color = activeTabColor;
        tabText.color = activeTabColor;
    }

    // Метод для разблокировки таба (можно вызвать извне при выполнении условий)
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