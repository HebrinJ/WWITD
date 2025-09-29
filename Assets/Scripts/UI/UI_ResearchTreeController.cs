using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���������� ������ ������������, ���������� �� ���������� ���������� � ����������
/// ����������� ����� ����������. ����������� ������� UI-������������� ����� � �����
/// ������������, ��������� �� ������������� � ����������� �������.
/// </summary>
/// <remarks>
/// <para><b>���� � �����������:</b> �������� ������������������ ������������ ��� �����������
/// ������������� ������������������ ������. �� ������� �� ����������� ���������� �������
/// � ����� ���� ��������������� � ������ ����������.</para>
/// <para><b>������������� �����������:</b></para>
/// <list type="bullet">
/// <item><description>�������� ������������� � ���������� �������������� ������ ������������</description></item>
/// <item><description>�� ��������� ������-������� ������������ (��� ��������������� ResearchManager)</description></item>
/// <item><description>������������ ������������ ������������ ������ ��� ����� �������</description></item>
/// <item><description>������������ �������� � ����������� UI-���������</description></item>
/// </list>
/// </remarks>
public class UI_ResearchTreeController : MonoBehaviour
{
    [Header("UI References")]
    /// <summary>
    /// ��������� ��� ���������� ���� ����� ������������.
    /// ��� ����������� �����, ���� � �������������� ����� �������� ��������� � ����� transform.
    /// </summary>
    [Tooltip("������������ ��������� ��� ���� ����� ������������.")]
    [SerializeField] private Transform branchesContainer;

    /// <summary>
    /// ������ UI-�������� ���� ������������.
    /// ������ ��������� ��������� UI_ResearchNode ��� ���������� ������.
    /// </summary>
    [Tooltip("������ UI-�������� ���� ������������.")]
    [SerializeField] private UI_ResearchNode researchNodePrefab;

    /// <summary>
    /// ������ �����-����������� ����� ������ ������������.
    /// ������������ ��� ����������� ����������� ������������������ ������������ � �����.
    /// </summary>
    [Tooltip("������ �����-����������� ����� ������ ������������.")]
    [SerializeField] private RectTransform connectorLinePrefab;

    [Header("Layout Settings")]
    /// <summary>
    /// �������������� ���������� ����� ������ � ����� ����� � ��������.
    /// ���������� �������� ����� ����������������� �������������� � �������������� �����������.
    /// </summary>
    [Tooltip("�������������� ���������� ����� ������ � ��������.")]
    [SerializeField] private float horizontalSpacing = 200f;

    /// <summary>
    /// ������������ ���������� ����� ������� ������������ � ��������.
    /// ���������� �������� ����� ������� ������� � ������������ �����������.
    /// </summary>
    [Tooltip("������������ ���������� ����� ������� � ��������.")]
    [SerializeField] private float verticalSpacing = 100f;

    /// <summary>
    /// ������� ��� ����� ������ ����� ������������ � �� UI-��������������.
    /// ������������ ������� ������ � UI-��������� �� ������ �����.
    /// </summary>
    /// <remarks>
    /// ����: ResearchNodeSO (������ ����)
    /// ��������: UI_ResearchNode (���������� �������������)
    /// </remarks>
    private Dictionary<ResearchNodeSO, UI_ResearchNode> nodeUIMap = new Dictionary<ResearchNodeSO, UI_ResearchNode>();

    /// <summary>
    /// ������� �������� ������� ������������.
    /// ����������, ����� ����� � ���� ���������� � ����������.
    /// </summary>
    private ResearchTabType currentTab = ResearchTabType.Towers;

    /// <summary>
    /// ��������� ����� ��� ������������� ������ ������������.
    /// ���������� �������� ��������� (��������, UI_ResearchPanelManager) ��� �������� ������.
    /// </summary>
    /// <example>
    /// Typical usage: ���������� ��� �������� ������ ������������.
    /// <code>researchTreeController.InitializeResearchTree();</code>
    /// </example>
    public void InitializeResearchTree()
    {
        ClearResearchTreeUI();
        CreateResearchTreeUI();
        Debug.Log("[UI_ResearchTreeController] ������ ������������ ����������������");
    }

    /// <summary>
    /// ��������� ����� ��� ������� ������ ������������.
    /// ���������� ��� �������� ������ ��� ����� ������� ��� ������������ ��������.
    /// </summary>
    public void CleanupResearchTree()
    {
        ClearResearchTreeUI();
        Debug.Log("[UI_ResearchTreeController] ������ ������������ �������");
    }

    /// <summary>
    /// ������������� �� ������� ����� ������� ������������ ��� ��������� �����������.
    /// ������������ ���������� ���������� ������ ��� ��������� �������� �������.
    /// </summary>
    private void OnEnable()
    {
        EventHub.OnResearchTabChanged += HandleTabChanged;
        Debug.Log("[UI_ResearchTreeController] �������� �� ������� ����� ������� ������������");
    }

    /// <summary>
    /// ������������ �� ������� ��� ����������� �����������.
    /// ������������� ������ ������ � ������������ ���������� UI.
    /// </summary>
    private void OnDisable()
    {
        EventHub.OnResearchTabChanged -= HandleTabChanged;
        CleanupResearchTree();
        Debug.Log("[UI_ResearchTreeController] �������� �� ������� ����� ������� ��������������");
    }

    /// <summary>
    /// ���������� ������� ����� �������� ������� ������������.
    /// ������������� UI ������ ���������� ��� ����������� ����������� ����� �������.
    /// </summary>
    /// <param name="tabType">��� �������, �� ������� ��������� ������������.</param>
    private void HandleTabChanged(ResearchTabType tabType)
    {
        currentTab = tabType;
        Debug.Log($"[UI_ResearchTreeController] �������� ������� �������� ��: {tabType}");

        ClearResearchTreeUI();
        CreateResearchTreeUI();
    }

    /// <summary>
    /// ������� UI-������������� ����� ������ ������������ ��� ������� �������� �������.
    /// ����������� ������ � ResearchManager � ���������� �������� ��������� �����.
    /// </summary>
    private void CreateResearchTreeUI()
    {
        // �������� ����������� ResearchManager
        if (ResearchManager.Instance == null)
        {
            Debug.LogError("[UI_ResearchTreeController] ResearchManager �� ������!");
            return;
        }

        // ��������� ����� ��� ������� �������
        ResearchBranchSO[] branches = ResearchManager.Instance.GetBranchesForTab(currentTab);

        if (branches == null || branches.Length == 0)
        {
            Debug.LogWarning($"[UI_ResearchTreeController] ��� ������� {currentTab} �� ������� ����� ������������");
            return;
        }

        Debug.Log($"[UI_ResearchTreeController] �������� ������ ��� ������� {currentTab}. ������� �����: {branches.Length}");

        // �������� UI ��� ������ ����� � ������������ �����������������
        for (int i = 0; i < branches.Length; i++)
        {
            CreateBranchUI(branches[i], i);
        }

        Debug.Log($"[UI_ResearchTreeController] ������ ������������ ������� �������. ����� �����: {nodeUIMap.Count}");
    }

    /// <summary>
    /// ������� UI-������������� ��������� ����� ������������.
    /// ������� ����������, ���� � �������������� ����� ��� ������������������ ������������.
    /// </summary>
    /// <param name="branchData">������ ����� ������������.</param>
    /// <param name="branchIndex">������ ����� ��� ������������� ����������������.</param>
    private void CreateBranchUI(ResearchBranchSO branchData, int branchIndex)
    {
        // �������� ���������� ������ �����
        if (branchData == null)
        {
            Debug.LogWarning("[UI_ResearchTreeController] ������� ������� UI ��� null �����");
            return;
        }

        if (branchData.ResearchNodes == null || branchData.ResearchNodes.Length == 0)
        {
            Debug.LogWarning($"[UI_ResearchTreeController] ����� {branchData.BranchName} �� �������� �����");
            return;
        }

        // �������� ��������� ���������� ��� �����
        GameObject branchContainer = new GameObject($"Branch_{branchData.BranchName}");
        branchContainer.transform.SetParent(branchesContainer, false);

        // �������� ���������� ��� ����� (������ ���� behind ����� � ��������)
        GameObject linesContainer = new GameObject("ConnectorLines");
        linesContainer.transform.SetParent(branchContainer.transform, false);
        linesContainer.transform.SetAsFirstSibling();

        // ������������ ���������������� �����
        float verticalPosition = -branchIndex * verticalSpacing;

        // �������� ����� ������������
        List<UI_ResearchNode> createdNodes = new List<UI_ResearchNode>();

        for (int i = 0; i < branchData.ResearchNodes.Length; i++)
        {
            ResearchNodeSO node = branchData.ResearchNodes[i];

            // �������� ���������� ������ ����
            if (node == null)
            {
                Debug.LogWarning($"[UI_ResearchTreeController] ������ null ���� � ����� {branchData.BranchName} �� ������� {i}");
                continue;
            }

            // �������� UI �������� ��� ����
            UI_ResearchNode nodeUI = Instantiate(researchNodePrefab, branchContainer.transform);
            if (nodeUI == null)
            {
                Debug.LogError("[UI_ResearchTreeController] �� ������� ������� UI_ResearchNode �� �������");
                continue;
            }

            // ������������� ����
            nodeUI.Initialize(node);

            // ���������������� ����
            RectTransform rect = nodeUI.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(i * horizontalSpacing, verticalPosition);

            // ���������� ����� ������ � UI
            nodeUIMap[node] = nodeUI;
            createdNodes.Add(nodeUI);

            Debug.Log($"[UI_ResearchTreeController] ������ ����: {node.NodeName} � ������� ({i}, {branchIndex})");
        }

        // �������� �������������� ����� ����� ������
        CreateConnectorLines(createdNodes, linesContainer.transform);

        Debug.Log($"[UI_ResearchTreeController] ����� {branchData.BranchName} �������. �����: {createdNodes.Count}");
    }

    /// <summary>
    /// ������� �������������� ����� ����� ������ ������������ � �����.
    /// ��������� ���������� ������������������ ������������ � ����������� ����� ������.
    /// </summary>
    /// <param name="nodes">������ ����� � �����.</param>
    /// <param name="linesContainer">��������� ��� ���������� �����.</param>
    private void CreateConnectorLines(List<UI_ResearchNode> nodes, Transform linesContainer)
    {
        // �������� ������� ����������� �����
        if (nodes == null || nodes.Count < 2)
        {
            Debug.LogWarning("[UI_ResearchTreeController] ������������ ����� ��� �������� �������������� �����");
            return;
        }

        if (connectorLinePrefab == null)
        {
            Debug.LogError("[UI_ResearchTreeController] �� �������� ������ �������������� �����!");
            return;
        }

        // �������� ����� ����� ����������������� ������
        for (int i = 1; i < nodes.Count; i++)
        {
            UI_ResearchNode fromNode = nodes[i - 1];
            UI_ResearchNode toNode = nodes[i];

            // �������� ���������� �����
            if (fromNode == null || toNode == null)
            {
                Debug.LogWarning($"[UI_ResearchTreeController] ������� �������� ����� ��-�� null ����� �� �������� {i - 1} -> {i}");
                continue;
            }

            CreateConnectorLine(fromNode, toNode, linesContainer);
        }

        Debug.Log($"[UI_ResearchTreeController] ������� �������������� �����: {nodes.Count - 1}");
    }

    /// <summary>
    /// ������� ��������� �������������� ����� ����� ����� ������ ������������.
    /// ������������ �������, ������ � rotation ����� �� ������ ������� �����.
    /// </summary>
    /// <param name="fromNode">�������� ����.</param>
    /// <param name="toNode">������� ����.</param>
    /// <param name="parent">������������ ��������� ��� �����.</param>
    private void CreateConnectorLine(UI_ResearchNode fromNode, UI_ResearchNode toNode, Transform parent)
    {
        RectTransform fromRect = fromNode.GetComponent<RectTransform>();
        RectTransform toRect = toNode.GetComponent<RectTransform>();

        // ��������� ������� ������� �����
        Vector2 fromPos = fromRect.position;
        Vector2 toPos = toRect.position;

        // �������� �����
        RectTransform connector = Instantiate(connectorLinePrefab, parent);
        connector.gameObject.SetActive(true);

        // ���������������� ����� ���������� ����� ������
        connector.position = (fromPos + toPos) / 2f;

        // ������ ����� � ���� �����
        float distance = Vector2.Distance(fromPos, toPos);
        float angle = Mathf.Atan2(toPos.y - fromPos.y, toPos.x - fromPos.x) * Mathf.Rad2Deg;

        // ��������� ������� � rotation �����
        connector.sizeDelta = new Vector2(distance, connector.sizeDelta.y);
        connector.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log($"[UI_ResearchTreeController] ������� �������������� ����� ������ {distance:F1} ��������");
    }

    /// <summary>
    /// ������� UI-������������� ������ ������������.
    /// ���������� ��� ��������� ����, ����� � ����������, ����������� �������.
    /// </summary>
    private void ClearResearchTreeUI()
    {
        // ��������� ���� �������� �������
        StopAllCoroutines();

        // ���������� ������� ������� �����
        var keysToRemove = new List<ResearchNodeSO>();
        foreach (var pair in nodeUIMap)
        {
            if (pair.Value == null)
            {
                keysToRemove.Add(pair.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            nodeUIMap.Remove(key);
        }

        // ���������� ����������� ���� �������� ��������
        for (int i = branchesContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = branchesContainer.GetChild(i);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        // ������� �������
        nodeUIMap.Clear();

        Debug.Log("[UI_ResearchTreeController] ������ ������������ ��������� �������");
    }
}