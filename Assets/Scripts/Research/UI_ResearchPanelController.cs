using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_ResearchPanelController : MonoBehaviour
{
    /*[Header("References")]
    [SerializeField] private ResearchBranchSO[] allBranches; // Все ветки исследований
    [SerializeField] private Transform branchesContainer;
    [SerializeField] private UI_ResearchNode researchNodePrefab;
    [SerializeField] private RectTransform connectorLinePrefab;

    [Header("Layout Settings")]
    [SerializeField] private float horizontalSpacing = 200f;
    [SerializeField] private float verticalSpacing = 100f;

    private ResearchTabType currentTab = ResearchTabType.Towers;

    private Dictionary<ResearchNodeSO, UI_ResearchNode> nodeUIMap = new Dictionary<ResearchNodeSO, UI_ResearchNode>();

    private void OnEnable()
    {
        EventHub.OnResearchTabChanged += HandleTabChanged;
        CreateResearchTreeUI();
    }

    private void OnDisable()
    {
        EventHub.OnResearchTabChanged -= HandleTabChanged;
        ClearResearchTreeUI();
    }

    private void HandleTabChanged(ResearchTabType tabType)
    {
        currentTab = tabType;
        ClearResearchTreeUI();
        CreateResearchTreeUI();
    }

    private void CreateResearchTreeUI()
    {
        ResearchBranchSO[] branches = ResearchManager.Instance.GetBranchesForTab(currentTab);

        if (branches == null || branches.Length == 0) return;

        foreach (ResearchBranchSO branch in branches)
        {
            CreateBranchUI(branch);
        }
    }

    private void CreateBranchUI(ResearchBranchSO branchData)
    {
        if (branchData.ResearchNodes == null || branchData.ResearchNodes.Length == 0) return;

        // Создаем контейнер для ветки
        GameObject branchContainer = new GameObject(branchData.BranchName);
        branchContainer.transform.SetParent(branchesContainer, false);

        // Создаем узлы и соединительные линии
        for (int i = 0; i < branchData.ResearchNodes.Length; i++)
        {
            ResearchNodeSO node = branchData.ResearchNodes[i];

            // Создаем UI элемент для узла
            UI_ResearchNode nodeUI = Instantiate(researchNodePrefab, branchContainer.transform);
            nodeUI.Initialize(node);
            nodeUIMap[node] = nodeUI;

            // Позиционируем узел
            RectTransform rectTransform = nodeUI.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(i * horizontalSpacing, 0);

            // Создаем соединительные линии между узлами
            if (i > 0)
            {
                CreateConnectorLine(branchData.ResearchNodes[i - 1], node, branchContainer.transform);
            }
        }
    }

    private void CreateConnectorLine(ResearchNodeSO fromNode, ResearchNodeSO toNode, Transform parent)
    {
        if (!nodeUIMap.ContainsKey(fromNode) || !nodeUIMap.ContainsKey(toNode)) return;

        RectTransform fromRect = nodeUIMap[fromNode].GetComponent<RectTransform>();
        RectTransform toRect = nodeUIMap[toNode].GetComponent<RectTransform>();

        RectTransform connector = Instantiate(connectorLinePrefab, parent);
        connector.gameObject.SetActive(true);

        // Позиционируем линию между узлами
        Vector2 fromPos = fromRect.anchoredPosition;
        Vector2 toPos = toRect.anchoredPosition;

        connector.anchoredPosition = (fromPos + toPos) / 2f;
        connector.sizeDelta = new Vector2(Vector2.Distance(fromPos, toPos), 5f);
        connector.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(toPos.y - fromPos.y, toPos.x - fromPos.x) * Mathf.Rad2Deg);
    }

    private void ClearResearchTreeUI()
    {
        nodeUIMap.Clear();
        foreach (Transform child in branchesContainer)
        {
            Destroy(child.gameObject);
        }
    }*/

    [Header("References")]
    [SerializeField] private Transform branchesContainer;
    [SerializeField] private UI_ResearchNode researchNodePrefab;
    [SerializeField] private RectTransform connectorLinePrefab;

    [Header("Layout Settings")]
    [SerializeField] private float horizontalSpacing = 200f;
    [SerializeField] private float verticalSpacing = 100f;

    private Dictionary<ResearchNodeSO, UI_ResearchNode> nodeUIMap = new Dictionary<ResearchNodeSO, UI_ResearchNode>();
    private ResearchTabType currentTab = ResearchTabType.Towers;

    private void OnEnable()
    {
        EventHub.OnResearchTabChanged += HandleTabChanged;
        CreateResearchTreeUI();
    }

    private void OnDisable()
    {
        EventHub.OnResearchTabChanged -= HandleTabChanged;
        ClearResearchTreeUI();
    }

    private void HandleTabChanged(ResearchTabType tabType)
    {
        currentTab = tabType;
        ClearResearchTreeUI();
        CreateResearchTreeUI();
    }

    private void CreateResearchTreeUI()
    {
        ResearchBranchSO[] branches = ResearchManager.Instance.GetBranchesForTab(currentTab);

        if (branches == null || branches.Length == 0)
        {
            Debug.Log($"No branches found for tab: {currentTab}");
            return;
        }

        foreach (ResearchBranchSO branch in branches)
        {
            CreateBranchUI(branch);
        }
    }

    private void CreateBranchUI(ResearchBranchSO branchData)
    {
        if (branchData.ResearchNodes == null || branchData.ResearchNodes.Length == 0) return;

        // Создаем контейнер для ветки
        GameObject branchContainer = new GameObject(branchData.BranchName);
        branchContainer.transform.SetParent(branchesContainer, false);

        // Добавляем вертикальный layout для узлов
        var verticalLayout = branchContainer.AddComponent<VerticalLayoutGroup>();
        verticalLayout.spacing = verticalSpacing;
        verticalLayout.childAlignment = TextAnchor.UpperLeft;

        // Создаем узлы
        for (int i = 0; i < branchData.ResearchNodes.Length; i++)
        {
            ResearchNodeSO node = branchData.ResearchNodes[i];

            // Создаем UI элемент для узла
            UI_ResearchNode nodeUI = Instantiate(researchNodePrefab, branchContainer.transform);
            nodeUI.Initialize(node);
            nodeUIMap[node] = nodeUI;
        }
    }

    private void ClearResearchTreeUI()
    {
        nodeUIMap.Clear();
        foreach (Transform child in branchesContainer)
        {
            Destroy(child.gameObject);
        }
    }
}