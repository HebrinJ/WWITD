using UnityEngine;

/// <summary>
/// ������� �������� ������ ������������, ���������� �� ���������� ���������� ���������, ���������
/// � ���������� ������������������ ����������. ������������ ������ ����� ���������� UI-������������
/// ������ ������������ � ������������ ��������������� � ���������� ���������� ����.
/// </summary>
/// <remarks>
/// <para><b>���� � �����������:</b> �������� ����������� ������������� ��� ���� ����������������� UI-�������.
/// ��������� ���������� ������, ��������� ������� ����������� � �������������� ����������� ���������� ��������.</para>
/// <para><b>��������������:</b></para>
/// <list type="bullet">
/// <item><description>��������������� � <see cref="GameStateManager"/> ��� �������� ������������ �������� ������</description></item>
/// <item><description>������������ ������ <see cref="UI_ResearchTreeController"/> ��� ���������� ������ ����������</description></item>
/// <item><description>���������������� � <see cref="GlobalResourceManager"/> ��� ����������� ����������� ���������� ��������</description></item>
/// <item><description>��������� ������ ������� ����� <see cref="EventHub"/> �� ���������� ��������� ������</description></item>
/// </list>
/// </remarks>
public class UI_ResearchPanelManager : MonoBehaviour
{
    /// <summary>
    /// ���������� ����� ������� � ���������� ��������� ������ ������������.
    /// ����������� ������������� ���������� ����������������� ����������� �� ����� ����� ����.
    /// </summary>
    public static UI_ResearchPanelManager Instance { get; private set; }

    [Header("Research UI Components")]
    /// <summary>
    /// ������ �� �������� GameObject ������ ������������.
    /// �������� ��� UI-��������, ��������� � ����������� ������������.
    /// </summary>
    [Tooltip("�������� GameObject ������ ������������.")]
    [SerializeField] private GameObject researchPanel;

    /// <summary>
    /// ������ �� ���������� ������ ������������, ���������� �� ���������� ����������
    /// � ���������� ������ ����������. ���������� ��� �������� � ���������� ������.
    /// </summary>
    [Tooltip("���������� ������ ������������, ����������� ������ � �������.")]
    [SerializeField] private UI_ResearchTreeController researchTreeController;

    /// <summary>
    /// ������ �� UI-��������� ����������� ���������� ��������.
    /// ������������ ���������� �������� ����� � ��������� ���������� �������� ��� ������������.
    /// </summary>
    [Tooltip("��������� ����������� ���������� �������� (���������� ������).")]
    [SerializeField] private UI_ResourceDisplay blueprintDisplay;

    /// <summary>
    /// ����, ����������� ������� �� � ������ ������ ������ ������������.
    /// ������������ ��� �������������� ���������� ���������� � ������������� ��������� UI.
    /// </summary>
    public bool IsResearchPanelOpen { get; private set; }

    /// <summary>
    /// �������������� singleton-��������� � ����������� ���������� ����� �������.
    /// ����������, ��� ���������� UI ������������ ����������� ��� �������� ����� �������.
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
    /// �������������� ���������� UI ������������ ��� ������.
    /// �������� ������ � ��������� �������� ���������.
    /// </summary>
    private void Start()
    {
        InitializeResearchPanel();
    }

    /// <summary>
    /// �������� ������ ������������ � ��������� ���������.
    /// �����������, ��� ������ ������� ��� ������ ���� ��� �������� �����.
    /// </summary>
    private void InitializeResearchPanel()
    {
        CloseResearchPanel();
        Debug.Log("[UI_ResearchPanelManager] ������ ������������ ���������������� � �������� ���������");
    }

    /// <summary>
    /// �������� ����� ��� ������������ ��������� ������ ������������ (�������/�������).
    /// ��������� �������� ����������� � ������������ ��� ��������� �������.
    /// </summary>
    /// <example>
    /// Typical usage: ���������� �� UI ������ "������������" �� �������������� �����.
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
    /// ��������� ������ ������������, ���� ��� �������� � ������� ��������� ����.
    /// ��������� �������� �����������, ���������� ������ � �������������� �������� ����������.
    /// </summary>
    /// <remarks>
    /// <para>������� �������� ������:</para>
    /// <list type="bullet">
    /// <item><description>���� �� ��������� � ��������� �������� ����</description></item>
    /// <item><description>������ ��� �� �������</description></item>
    /// <item><description>��� ����������� ������ ����������������</description></item>
    /// </list>
    /// </remarks>
    public void OpenResearchPanel()
    {
        // �������� ������� ����������� ������
        if (researchPanel == null)
        {
            Debug.LogError("[UI_ResearchPanelManager] �� ��������� ������ �� researchPanel!");
            return;
        }

        // ������ �� ���������� ��������
        if (IsResearchPanelOpen)
        {
            Debug.LogWarning("[UI_ResearchPanelManager] ������ ������������ ��� �������");
            return;
        }

        // �������� ����������� � ������� ��������� ����
        if (!CanOpenResearchPanel())
        {
            Debug.LogWarning($"[UI_ResearchPanelManager] ���������� ������� ������ ������������ � ������� ��������� ����: {GameStateManager.Instance.CurrentState}");
            return;
        }

        // ��������� ������ � ��������� �����
        researchPanel.SetActive(true);
        IsResearchPanelOpen = true;

        // ������������� �������� �����������
        InitializeResearchComponents();

        Debug.Log("[UI_ResearchPanelManager] ������ ������������ ������� �������");
    }

    /// <summary>
    /// ��������� ������ ������������ � �������� ��� ������� � ������������� ���������.
    /// ��������� ������� ��������� ������ � ����������� UI-���������.
    /// </summary>
    public void CloseResearchPanel()
    {
        // �������� ������� ����������� ������
        if (researchPanel == null)
        {
            Debug.LogError("[UI_ResearchPanelManager] �� ��������� ������ �� researchPanel!");
            return;
        }

        // ������ �� ���������� ��������
        if (!IsResearchPanelOpen)
        {
            Debug.LogWarning("[UI_ResearchPanelManager] ������ ������������ ��� �������");
            return;
        }

        // ����������� ������ � ����� �����
        researchPanel.SetActive(false);
        IsResearchPanelOpen = false;

        // ������� ��������� ������ �������� �����������
        CleanupResearchComponents();

        Debug.Log("[UI_ResearchPanelManager] ������ ������������ ������� �������");
    }

    /// <summary>
    /// ���������, ����� �� ������� ������ ������������ � ������� ��������� ����.
    /// ���������� ������-������ ����������� ������������������ ����������.
    /// </summary>
    /// <returns>true - ���� ������ ����� �������, false - � ��������� ������.</returns>
    private bool CanOpenResearchPanel()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogError("[UI_ResearchPanelManager] GameStateManager �� ������!");
            return false;
        }

        var currentState = GameStateManager.Instance.CurrentState;

        // ������ ������������ ���������� � ������� ���� � �� �����
        return currentState != GameState.MainMenu && currentState != GameState.Paused;
    }

    /// <summary>
    /// �������������� ��� �������� ���������� ������ ������������ ��� ��������.
    /// ������������ ���������������� ������������� ��������� ������.
    /// </summary>
    private void InitializeResearchComponents()
    {
        // ������������� ������ ������������
        if (researchTreeController != null)
        {
            researchTreeController.InitializeResearchTree();
        }
        else
        {
            Debug.LogWarning("[UI_ResearchPanelManager] ResearchTreeController �� ��������!");
        }

        // ���������� ����������� ��������
        UpdateBlueprintDisplay();
    }

    /// <summary>
    /// ��������� ������� ���� �������� ����������� ������ ������������ ��� ��������.
    /// ����������� ��������� ������� � ���������� ���������.
    /// </summary>
    private void CleanupResearchComponents()
    {
        // ������� ������ ������������
        if (researchTreeController != null)
        {
            researchTreeController.CleanupResearchTree();
        }
    }

    /// <summary>
    /// ��������� ����������� ���������� �������� � UI.
    /// ����������� ���������� ������ � GlobalResourceManager � �������������� ���������� �������������.
    /// </summary>
    public void UpdateBlueprintDisplay()
    {
        if (blueprintDisplay == null)
        {
            Debug.LogWarning("[UI_ResearchPanelManager] BlueprintDisplay �� ��������!");
            return;
        }

        if (GlobalResourceManager.Instance == null)
        {
            Debug.LogError("[UI_ResearchPanelManager] GlobalResourceManager �� ������!");
            return;
        }

        int blueprints = GlobalResourceManager.Instance.GetResourceAmount(ResourceType.Blueprints);
        blueprintDisplay.UpdateResourceDisplay(ResourceType.Blueprints, blueprints);

        Debug.Log($"[UI_ResearchPanelManager] ����������� �������� ���������: {blueprints}");
    }

    /// <summary>
    /// ������������� �� ������� ��������� ���������� �������� ��� ��������� �������.
    /// ������������ ���������� ���������� UI ��� ��������� ���������� ��������.
    /// </summary>
    private void OnEnable()
    {
        EventHub.OnGlobalResourceChanged += HandleGlobalResourceChanged;
        Debug.Log("[UI_ResearchPanelManager] �������� �� ������� ���������� �������� ������������");
    }

    /// <summary>
    /// ������������ �� ������� ��� ����������� ������� ��� �������������� ������ ������.
    /// ����������� ���������� ������������ �������� ��� ����������� �������.
    /// </summary>
    private void OnDisable()
    {
        EventHub.OnGlobalResourceChanged -= HandleGlobalResourceChanged;
        Debug.Log("[UI_ResearchPanelManager] �������� �� ������� ���������� �������� ��������������");
    }

    /// <summary>
    /// ���������� ������� ��������� ���������� ��������.
    /// ������������� ��������� ����������� �������� ��� ��������� �� ����������.
    /// </summary>
    /// <param name="resourceType">��� ������������� �������.</param>
    /// <param name="newAmount">����� ���������� �������.</param>
    private void HandleGlobalResourceChanged(ResourceType resourceType, int newAmount)
    {
        // ��������� ������ �� ��������� �������� � ������ ����� ������ �������
        if (resourceType == ResourceType.Blueprints && IsResearchPanelOpen)
        {
            UpdateBlueprintDisplay();
            Debug.Log($"[UI_ResearchPanelManager] ���������� ��������� ��������: {newAmount}");
        }
    }
}