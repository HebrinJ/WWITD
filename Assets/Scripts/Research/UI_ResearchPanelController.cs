using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Контроллер панели исследований, отвечающий за визуальное построение древа технологий.
/// Динамически создает UI-представление веток и узлов исследований для текущей активной вкладки.
/// Управляет расположением узлов, соединительными линиями и обновлением интерфейса при смене вкладок.
/// </summary>
public class UI_ResearchPanelController : MonoBehaviour
{
    [Header("References")]
    /// <summary>
    /// Контейнер для размещения веток исследований.
    /// Все создаваемые ветки и узлы будут дочерними к этому transform.
    /// </summary>
    [Tooltip("Родительский контейнер для всех веток исследований.")]
    [SerializeField] private Transform branchesContainer;

    /// <summary>
    /// Префаб UI-элемента узла исследования.
    /// Должен содержать компонент UI_ResearchNode.
    /// </summary>
    [Tooltip("Префаб UI-элемента узла исследования.")]
    [SerializeField] private UI_ResearchNode researchNodePrefab;

    /// <summary>
    /// Префаб линии-соединителя между узлами исследований.
    /// Используется для визуального отображения последовательности исследований.
    /// </summary>
    [Tooltip("Префаб линии-соединителя между узлами.")]
    [SerializeField] private RectTransform connectorLinePrefab;

    [Header("Layout Settings")]
    /// <summary>
    /// Горизонтальное расстояние между узлами в одной ветке.
    /// Определяет интервал между последовательными исследованиями.
    /// </summary>
    [Tooltip("Горизонтальное расстояние между узлами в пикселях.")]
    [SerializeField] private float horizontalSpacing = 200f;

    /// <summary>
    /// Вертикальное расстояние между ветками исследований.
    /// Определяет интервал между разными ветками в одной вкладке.
    /// </summary>
    [Tooltip("Вертикальное расстояние между ветками в пикселях.")]
    [SerializeField] private float verticalSpacing = 100f;

    /// <summary>
    /// Словарь для связи данных узлов с их UI-представлением.
    /// Ключ: ResearchNodeSO (данные узла)
    /// Значение: UI_ResearchNode (визуальное представление)
    /// </summary>
    private Dictionary<ResearchNodeSO, UI_ResearchNode> nodeUIMap = new Dictionary<ResearchNodeSO, UI_ResearchNode>();

    /// <summary>
    /// Текущая активная вкладка исследований.
    /// Определяет, какие ветки и узлы отображать в интерфейсе.
    /// </summary>
    private ResearchTabType currentTab = ResearchTabType.Towers;

    /// <summary>
    /// Подписывается на события смены вкладки и создает UI при активации панели.
    /// </summary>
    private void OnEnable()
    {
        EventHub.OnResearchTabChanged += HandleTabChanged;
        CreateResearchTreeUI();
    }

    /// <summary>
    /// Отписывается от событий и очищает UI при деактивации панели.
    /// </summary>
    private void OnDisable()
    {
        EventHub.OnResearchTabChanged -= HandleTabChanged;
        ClearResearchTreeUI();
    }

    /// <summary>
    /// Обработчик смены активной вкладки исследований.
    /// Перестраивает UI дерева технологий для новой вкладки.
    /// </summary>
    /// <param name="tabType">Тип вкладки, на которую произошло переключение.</param>
    private void HandleTabChanged(ResearchTabType tabType)
    {
        currentTab = tabType;
        ClearResearchTreeUI();
        CreateResearchTreeUI();
    }

    /// <summary>
    /// Создает UI-представление всего дерева исследований для текущей вкладки.
    /// Запрашивает у ResearchManager ветки, относящиеся к активной вкладке.
    /// </summary>
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

    /// <summary>
    /// Создает UI-представление отдельной ветки исследований.
    /// Создает контейнеры, узлы и соединительные линии для последовательности исследований.
    /// </summary>
    /// <param name="branchData">Данные ветки исследований.</param>
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

        // Создаем узлы с фиксированными позициями
        List<UI_ResearchNode> createdNodes = new List<UI_ResearchNode>();

        for (int i = 0; i < branchData.ResearchNodes.Length; i++)
        {
            ResearchNodeSO node = branchData.ResearchNodes[i];

            // Создаем UI элемент для узла
            UI_ResearchNode nodeUI = Instantiate(researchNodePrefab, branchContainer.transform);
            nodeUI.Initialize(node);

            // Позиционируем
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

    /// <summary>
    /// Очищает UI-представление дерева исследований.
    /// Уничтожает все созданные узлы, линии и контейнеры.
    /// </summary>
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

    /// <summary>
    /// Создает соединительную линию между двумя узлами исследований.
    /// Визуально показывает последовательность исследований в ветке.
    /// </summary>
    /// <param name="fromNode">Исходный узел.</param>
    /// <param name="toNode">Целевой узел.</param>
    /// <param name="parent">Родительский контейнер для линии.</param>
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