using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Контроллер дерева исследований, отвечающий за визуальное построение и управление
/// интерфейсом древа технологий. Динамически создает UI-представление веток и узлов
/// исследований, управляет их расположением и визуальными связями.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является специализированным контроллером для визуального
/// представления исследовательского дерева. Не зависит от глобального управления панелью
/// и может быть переиспользован в других контекстах.</para>
/// <para><b>Архитектурные особенности:</b></para>
/// <list type="bullet">
/// <item><description>Работает исключительно с визуальным представлением данных исследований</description></item>
/// <item><description>Не управляет бизнес-логикой исследований (это ответственность ResearchManager)</description></item>
/// <item><description>Поддерживает динамическое перестроение дерева при смене вкладок</description></item>
/// <item><description>Оптимизирует создание и уничтожение UI-элементов</description></item>
/// </list>
/// </remarks>
public class UI_ResearchTreeController : MonoBehaviour
{
    [Header("UI References")]
    /// <summary>
    /// Контейнер для размещения всех веток исследований.
    /// Все создаваемые ветки, узлы и соединительные линии являются дочерними к этому transform.
    /// </summary>
    [Tooltip("Родительский контейнер для всех веток исследований.")]
    [SerializeField] private Transform branchesContainer;

    /// <summary>
    /// Префаб UI-элемента узла исследования.
    /// Должен содержать компонент UI_ResearchNode для корректной работы.
    /// </summary>
    [Tooltip("Префаб UI-элемента узла исследования.")]
    [SerializeField] private UI_ResearchNode researchNodePrefab;

    /// <summary>
    /// Префаб линии-соединителя между узлами исследований.
    /// Используется для визуального отображения последовательности исследований в ветке.
    /// </summary>
    [Tooltip("Префаб линии-соединителя между узлами исследований.")]
    [SerializeField] private RectTransform connectorLinePrefab;

    [Header("Layout Settings")]
    /// <summary>
    /// Горизонтальное расстояние между узлами в одной ветке в пикселях.
    /// Определяет интервал между последовательными исследованиями в горизонтальном направлении.
    /// </summary>
    [Tooltip("Горизонтальное расстояние между узлами в пикселях.")]
    [SerializeField] private float horizontalSpacing = 200f;

    /// <summary>
    /// Вертикальное расстояние между ветками исследований в пикселях.
    /// Определяет интервал между разными ветками в вертикальном направлении.
    /// </summary>
    [Tooltip("Вертикальное расстояние между ветками в пикселях.")]
    [SerializeField] private float verticalSpacing = 100f;

    /// <summary>
    /// Словарь для связи данных узлов исследований с их UI-представлением.
    /// Обеспечивает быстрый доступ к UI-элементам по данным узлов.
    /// </summary>
    /// <remarks>
    /// Ключ: ResearchNodeSO (данные узла)
    /// Значение: UI_ResearchNode (визуальное представление)
    /// </remarks>
    private Dictionary<ResearchNodeSO, UI_ResearchNode> nodeUIMap = new Dictionary<ResearchNodeSO, UI_ResearchNode>();

    /// <summary>
    /// Текущая активная вкладка исследований.
    /// Определяет, какие ветки и узлы отображать в интерфейсе.
    /// </summary>
    private ResearchTabType currentTab = ResearchTabType.Towers;

    /// <summary>
    /// Публичный метод для инициализации дерева исследований.
    /// Вызывается внешними системами (например, UI_ResearchPanelManager) при открытии панели.
    /// </summary>
    /// <example>
    /// Typical usage: Вызывается при открытии панели исследований.
    /// <code>researchTreeController.InitializeResearchTree();</code>
    /// </example>
    public void InitializeResearchTree()
    {
        ClearResearchTreeUI();
        CreateResearchTreeUI();
        Debug.Log("[UI_ResearchTreeController] Дерево исследований инициализировано");
    }

    /// <summary>
    /// Публичный метод для очистки дерева исследований.
    /// Вызывается при закрытии панели или смене вкладки для освобождения ресурсов.
    /// </summary>
    public void CleanupResearchTree()
    {
        ClearResearchTreeUI();
        Debug.Log("[UI_ResearchTreeController] Дерево исследований очищено");
    }

    /// <summary>
    /// Подписывается на события смены вкладки исследований при активации контроллера.
    /// Обеспечивает реактивное обновление дерева при изменении активной вкладки.
    /// </summary>
    private void OnEnable()
    {
        EventHub.OnResearchTabChanged += HandleTabChanged;
        Debug.Log("[UI_ResearchTreeController] Подписка на события смены вкладок активирована");
    }

    /// <summary>
    /// Отписывается от событий при деактивации контроллера.
    /// Предотвращает утечки памяти и некорректное обновление UI.
    /// </summary>
    private void OnDisable()
    {
        EventHub.OnResearchTabChanged -= HandleTabChanged;
        CleanupResearchTree();
        Debug.Log("[UI_ResearchTreeController] Подписка на события смены вкладок деактивирована");
    }

    /// <summary>
    /// Обработчик события смены активной вкладки исследований.
    /// Перестраивает UI дерева технологий для отображения содержимого новой вкладки.
    /// </summary>
    /// <param name="tabType">Тип вкладки, на которую произошло переключение.</param>
    private void HandleTabChanged(ResearchTabType tabType)
    {
        currentTab = tabType;
        Debug.Log($"[UI_ResearchTreeController] Активная вкладка изменена на: {tabType}");

        ClearResearchTreeUI();
        CreateResearchTreeUI();
    }

    /// <summary>
    /// Создает UI-представление всего дерева исследований для текущей активной вкладки.
    /// Запрашивает данные у ResearchManager и делегирует создание отдельных веток.
    /// </summary>
    private void CreateResearchTreeUI()
    {
        // Проверка доступности ResearchManager
        if (ResearchManager.Instance == null)
        {
            Debug.LogError("[UI_ResearchTreeController] ResearchManager не найден!");
            return;
        }

        // Получение веток для текущей вкладки
        ResearchBranchSO[] branches = ResearchManager.Instance.GetBranchesForTab(currentTab);

        if (branches == null || branches.Length == 0)
        {
            Debug.LogWarning($"[UI_ResearchTreeController] Для вкладки {currentTab} не найдено веток исследований");
            return;
        }

        Debug.Log($"[UI_ResearchTreeController] Создание дерева для вкладки {currentTab}. Найдено веток: {branches.Length}");

        // Создание UI для каждой ветки с вертикальным позиционированием
        for (int i = 0; i < branches.Length; i++)
        {
            CreateBranchUI(branches[i], i);
        }

        Debug.Log($"[UI_ResearchTreeController] Дерево исследований успешно создано. Всего узлов: {nodeUIMap.Count}");
    }

    /// <summary>
    /// Создает UI-представление отдельной ветки исследований.
    /// Создает контейнеры, узлы и соединительные линии для последовательности исследований.
    /// </summary>
    /// <param name="branchData">Данные ветки исследований.</param>
    /// <param name="branchIndex">Индекс ветки для вертикального позиционирования.</param>
    private void CreateBranchUI(ResearchBranchSO branchData, int branchIndex)
    {
        // Проверка валидности данных ветки
        if (branchData == null)
        {
            Debug.LogWarning("[UI_ResearchTreeController] Попытка создать UI для null ветки");
            return;
        }

        if (branchData.ResearchNodes == null || branchData.ResearchNodes.Length == 0)
        {
            Debug.LogWarning($"[UI_ResearchTreeController] Ветка {branchData.BranchName} не содержит узлов");
            return;
        }

        // Создание основного контейнера для ветки
        GameObject branchContainer = new GameObject($"Branch_{branchData.BranchName}");
        branchContainer.transform.SetParent(branchesContainer, false);

        // Создание контейнера для линий (должен быть behind узлов в иерархии)
        GameObject linesContainer = new GameObject("ConnectorLines");
        linesContainer.transform.SetParent(branchContainer.transform, false);
        linesContainer.transform.SetAsFirstSibling();

        // Вертикальное позиционирование ветки
        float verticalPosition = -branchIndex * verticalSpacing;

        // Создание узлов исследований
        List<UI_ResearchNode> createdNodes = new List<UI_ResearchNode>();

        for (int i = 0; i < branchData.ResearchNodes.Length; i++)
        {
            ResearchNodeSO node = branchData.ResearchNodes[i];

            // Проверка валидности данных узла
            if (node == null)
            {
                Debug.LogWarning($"[UI_ResearchTreeController] Найден null узел в ветке {branchData.BranchName} на позиции {i}");
                continue;
            }

            // Создание UI элемента для узла
            UI_ResearchNode nodeUI = Instantiate(researchNodePrefab, branchContainer.transform);
            if (nodeUI == null)
            {
                Debug.LogError("[UI_ResearchTreeController] Не удалось создать UI_ResearchNode из префаба");
                continue;
            }

            // Инициализация узла
            nodeUI.Initialize(node);

            // Позиционирование узла
            RectTransform rect = nodeUI.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(i * horizontalSpacing, verticalPosition);

            // Сохранение связи данных и UI
            nodeUIMap[node] = nodeUI;
            createdNodes.Add(nodeUI);

            Debug.Log($"[UI_ResearchTreeController] Создан узел: {node.NodeName} в позиции ({i}, {branchIndex})");
        }

        // Создание соединительных линий между узлами
        CreateConnectorLines(createdNodes, linesContainer.transform);

        Debug.Log($"[UI_ResearchTreeController] Ветка {branchData.BranchName} создана. Узлов: {createdNodes.Count}");
    }

    /// <summary>
    /// Создает соединительные линии между узлами исследований в ветке.
    /// Визуально показывает последовательность исследований и зависимости между узлами.
    /// </summary>
    /// <param name="nodes">Список узлов в ветке.</param>
    /// <param name="linesContainer">Контейнер для размещения линий.</param>
    private void CreateConnectorLines(List<UI_ResearchNode> nodes, Transform linesContainer)
    {
        // Проверка наличия необходимых узлов
        if (nodes == null || nodes.Count < 2)
        {
            Debug.LogWarning("[UI_ResearchTreeController] Недостаточно узлов для создания соединительных линий");
            return;
        }

        if (connectorLinePrefab == null)
        {
            Debug.LogError("[UI_ResearchTreeController] Не назначен префаб соединительной линии!");
            return;
        }

        // Создание линий между последовательными узлами
        for (int i = 1; i < nodes.Count; i++)
        {
            UI_ResearchNode fromNode = nodes[i - 1];
            UI_ResearchNode toNode = nodes[i];

            // Проверка валидности узлов
            if (fromNode == null || toNode == null)
            {
                Debug.LogWarning($"[UI_ResearchTreeController] Пропуск создания линии из-за null узлов на позициях {i - 1} -> {i}");
                continue;
            }

            CreateConnectorLine(fromNode, toNode, linesContainer);
        }

        Debug.Log($"[UI_ResearchTreeController] Создано соединительных линий: {nodes.Count - 1}");
    }

    /// <summary>
    /// Создает отдельную соединительную линию между двумя узлами исследований.
    /// Рассчитывает позицию, размер и rotation линии на основе позиций узлов.
    /// </summary>
    /// <param name="fromNode">Исходный узел.</param>
    /// <param name="toNode">Целевой узел.</param>
    /// <param name="parent">Родительский контейнер для линии.</param>
    private void CreateConnectorLine(UI_ResearchNode fromNode, UI_ResearchNode toNode, Transform parent)
    {
        RectTransform fromRect = fromNode.GetComponent<RectTransform>();
        RectTransform toRect = toNode.GetComponent<RectTransform>();

        // Получение мировых позиций узлов
        Vector2 fromPos = fromRect.position;
        Vector2 toPos = toRect.position;

        // Создание линии
        RectTransform connector = Instantiate(connectorLinePrefab, parent);
        connector.gameObject.SetActive(true);

        // Позиционирование линии посередине между узлами
        connector.position = (fromPos + toPos) / 2f;

        // Расчет длины и угла линии
        float distance = Vector2.Distance(fromPos, toPos);
        float angle = Mathf.Atan2(toPos.y - fromPos.y, toPos.x - fromPos.x) * Mathf.Rad2Deg;

        // Установка размера и rotation линии
        connector.sizeDelta = new Vector2(distance, connector.sizeDelta.y);
        connector.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log($"[UI_ResearchTreeController] Создана соединительная линия длиной {distance:F1} пикселей");
    }

    /// <summary>
    /// Очищает UI-представление дерева исследований.
    /// Уничтожает все созданные узлы, линии и контейнеры, освобождает ресурсы.
    /// </summary>
    private void ClearResearchTreeUI()
    {
        // Остановка всех активных корутин
        StopAllCoroutines();

        // Безопасная очистка словаря узлов
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

        // Безопасное уничтожение всех дочерних объектов
        for (int i = branchesContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = branchesContainer.GetChild(i);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        // Очистка словаря
        nodeUIMap.Clear();

        Debug.Log("[UI_ResearchTreeController] Дерево исследований полностью очищено");
    }
}