using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance { get; private set; }

    [Header("Global UI Windows")]
    [SerializeField] private GameObject researchPanel;

    [Header("Resource Displays")]
    [SerializeField] private UI_ResourceDisplay blueprintDisplay;

    //public bool IsResearchPanelOpen { get; private set; }
    public bool IsResearchPanelOpen;

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

    private void Start()
    {
        InitializeGlobalUI();
    }

    private void InitializeGlobalUI()
    {
        CloseResearchPanel();
    }

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

    public void CloseResearchPanel()
    {
        if (researchPanel == null) return;
        if (!IsResearchPanelOpen) return;

        researchPanel.SetActive(false);
        IsResearchPanelOpen = false;

        Debug.Log("Research panel closed");
    }

    public void UpdateBlueprintDisplay()
    {
        if (blueprintDisplay != null && GlobalResourceManager.Instance != null)
        {
            int blueprints = GlobalResourceManager.Instance.GetResourceAmount(ResourceType.Blueprints);
            blueprintDisplay.UpdateResourceDisplay(ResourceType.Blueprints, blueprints);
        }
    }

    private void OnEnable()
    {
        EventHub.OnGlobalResourceChanged += HandleGlobalResourceChanged;
    }

    private void OnDisable()
    {
        EventHub.OnGlobalResourceChanged -= HandleGlobalResourceChanged;
    }

    private void HandleGlobalResourceChanged(ResourceType type, int amount)
    {
        if (type == ResourceType.Blueprints && IsResearchPanelOpen)
        {
            UpdateBlueprintDisplay();
        }
    }
}