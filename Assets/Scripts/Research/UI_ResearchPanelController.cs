using System.Collections.Generic;
using UnityEngine;

public class UI_ResearchPanelController : MonoBehaviour
{   
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

        // Создаем основной контейнер для ветки
        GameObject branchContainer = new GameObject(branchData.BranchName);
        branchContainer.transform.SetParent(branchesContainer, false);

        // Создаем контейнер для линий (должен быть behind узлов)
        GameObject linesContainer = new GameObject("LinesContainer");
        linesContainer.transform.SetParent(branchContainer.transform, false);
        linesContainer.transform.SetAsFirstSibling();

        // Создаем узлы вручную с фиксированными позициями
        List<UI_ResearchNode> createdNodes = new List<UI_ResearchNode>();

        for (int i = 0; i < branchData.ResearchNodes.Length; i++)
        {
            ResearchNodeSO node = branchData.ResearchNodes[i];

            // Создаем UI элемент для узла
            UI_ResearchNode nodeUI = Instantiate(researchNodePrefab, branchContainer.transform);
            nodeUI.Initialize(node);

            // Позиционируем вручную
            RectTransform rect = nodeUI.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(i * horizontalSpacing, 0);

            nodeUIMap[node] = nodeUI;
            createdNodes.Add(nodeUI);
        }

        // Создаем соединительные линии (немедленно)
        for (int i = 1; i < createdNodes.Count; i++)
        {
            if (createdNodes[i - 1] != null && createdNodes[i] != null)
            {
                CreateConnectorLine(createdNodes[i - 1], createdNodes[i], linesContainer.transform);
            }
        }
    }

    private void ClearResearchTreeUI()
    {
        StopAllCoroutines();

        // Безопасно очищаем словарь
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

        // Безопасно уничтожаем дочерние объекты
        for (int i = branchesContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = branchesContainer.GetChild(i);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        nodeUIMap.Clear();
    }

    private void CreateConnectorLine(UI_ResearchNode fromNode, UI_ResearchNode toNode, Transform parent)
    {
        RectTransform fromRect = fromNode.GetComponent<RectTransform>();
        RectTransform toRect = toNode.GetComponent<RectTransform>();

        // Получаем мировые позиции
        Vector2 fromPos = fromRect.position;
        Vector2 toPos = toRect.position;

        // Создаем линию
        RectTransform connector = Instantiate(connectorLinePrefab, parent);
        connector.gameObject.SetActive(true);

        // Позиционируем линию
        connector.position = (fromPos + toPos) / 2f;
        connector.sizeDelta = new Vector2(Vector2.Distance(fromPos, toPos), 5f);
        connector.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(toPos.y - fromPos.y, toPos.x - fromPos.x) * Mathf.Rad2Deg);
    }

}