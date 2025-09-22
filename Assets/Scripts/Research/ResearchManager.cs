using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Главный менеджер системы исследований игры.
/// Управляет всей логикой исследований: состояниями узлов, требованиями, применением эффектов.
/// Координирует взаимодействие между ResearchStateSO, ResearchEffectsManager и UI.
/// Сохраняется между сценами (DontDestroyOnLoad) для обеспечения непрерывности прогрессии.
/// </summary>
public class ResearchManager : MonoBehaviour
{
    /// <summary>
    /// Глобальная точка доступа к экземпляру ResearchManager.
    /// Гарантирует единообразное управление исследованиями на всех сценах.
    /// </summary>
    public static ResearchManager Instance { get; private set; }

    /// <summary>
    /// Ссылка на ScriptableObject, хранящий состояние исследований игрока.
    /// Управляет сохранением и загрузкой прогресса между сессиями.
    /// </summary>
    [SerializeField] private ResearchStateSO researchState;

    /// <summary>
    /// Массив всех веток исследований в игре.
    /// Должен быть заполнен в инспекторе ссылками на все ResearchBranchSO.
    /// </summary>
    [SerializeField] private ResearchBranchSO[] allBranches;

    [Header("Research Tabs")]
    /// <summary>
    /// Массив вкладок/разделов исследований для организации интерфейса.
    /// Определяет категоризацию исследований (Башни, Инженерия, Способности).
    /// </summary>
    [SerializeField] private ResearchTabSO[] researchTabs;

    /// <summary>
    /// Событие, вызываемое при изменении состояния любого узла исследования.
    /// Параметры: идентификатор узла, новое состояние.
    /// Используется UI для обновления визуального представления.
    /// </summary>
    public static event System.Action<string, ResearchNodeState> OnNodeStateChanged;

    /// <summary>
    /// Инициализирует singleton-экземпляр и настраивает начальное состояние исследований.
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

        InitializeDefaultStates();
    }

    /// <summary>
    /// Подписывается на события изменения ресурсов при активации менеджера.
    /// </summary>
    private void OnEnable()
    {
        EventHub.OnGlobalResourceChanged += HandleResourceChanged;
    }

    /// <summary>
    /// Отписывается от событий при деактивации менеджера.
    /// </summary>
    private void OnDisable()
    {
        EventHub.OnGlobalResourceChanged -= HandleResourceChanged;
    }

    /// <summary>
    /// Обработчик изменения глобальных ресурсов.
    /// Обновляет состояния всех узлов при изменении количества чертежей.
    /// </summary>
    /// <param name="resourceType">Тип изменившегося ресурса.</param>
    /// <param name="amount">Новое количество ресурса.</param>
    private void HandleResourceChanged(ResourceType resourceType, int amount)
    {
        if (resourceType == ResourceType.Blueprints)
        {
            UpdateAllNodesStates();
        }
    }

    /// <summary>
    /// Инициализирует состояния по умолчанию для всех узлов исследований.
    /// Узлы без требований помечаются как доступные с начала игры.
    /// </summary>
    private void InitializeDefaultStates()
    {
        // Инициализируем состояния по умолчанию для всех узлов
        foreach (ResearchBranchSO branch in allBranches)
        {
            foreach (ResearchNodeSO node in branch.ResearchNodes)
            {
                ResearchNodeState currentState = researchState.GetNodeState(node.name);

                // Если состояния нет в SO, устанавливаем дефолтное
                if (currentState == ResearchNodeState.Locked &&
                    (node.PrerequisiteNodes == null || node.PrerequisiteNodes.Length == 0))
                {
                    researchState.SetNodeState(node.name, ResearchNodeState.Available);
                }
            }
        }
    }

    /// <summary>
    /// Возвращает текущее состояние указанного узла исследования.
    /// Вычисляет состояние на основе требований и доступных ресурсов.
    /// </summary>
    /// <param name="node">Узел исследования для проверки.</param>
    /// <returns>Текущее состояние узла.</returns>
    public ResearchNodeState GetNodeState(ResearchNodeSO node)
    {
        if (node == null) return ResearchNodeState.Locked;

        ResearchNodeState state = researchState.GetNodeState(node.name);

        // Если узел не исследован, вычисляем его текущее состояние
        if (state != ResearchNodeState.Researched)
        {
            state = CalculateNodeState(node);
            // Сохраняем вычисленное состояние (но не перезаписываем Researched)
            if (researchState.GetNodeState(node.name) != ResearchNodeState.Researched)
            {
                researchState.SetNodeState(node.name, state);
            }
        }

        return state;
    }

    /// <summary>
    /// Вычисляет состояние узла на основе требований и доступных ресурсов.
    /// </summary>
    /// <param name="node">Узел для расчета.</param>
    /// <returns>Рассчитанное состояние узла.</returns>
    private ResearchNodeState CalculateNodeState(ResearchNodeSO node)
    {
        // Проверяем требования
        if (node.PrerequisiteNodes != null && node.PrerequisiteNodes.Length > 0)
        {
            foreach (ResearchNodeSO prereq in node.PrerequisiteNodes)
            {
                if (researchState.GetNodeState(prereq.name) != ResearchNodeState.Researched)
                {
                    return ResearchNodeState.Locked;
                }
            }
        }

        int currentBlueprints = GlobalResourceManager.Instance?.GetResourceAmount(ResourceType.Blueprints) ?? 0;
        if (currentBlueprints >= node.BlueprintCost)
        {
            return ResearchNodeState.Available;
        }
        else
        {
            return ResearchNodeState.Locked; // Или можно создать отдельный статус "NoResources"
        }
    }

    /// <summary>
    /// Проверяет, можно ли исследовать указанный узел в текущий момент.
    /// </summary>
    /// <param name="node">Узел для проверки.</param>
    /// <returns>True если узел доступен для исследования.</returns>
    public bool CanResearch(ResearchNodeSO node)
    {
        return GetNodeState(node) == ResearchNodeState.Available;
    }

    /// <summary>
    /// Пытается исследовать указанный узел.
    /// Проверяет доступность, списывает ресурсы и применяет эффекты.
    /// </summary>
    /// <param name="node">Узел для исследования.</param>
    /// <returns>True если исследование прошло успешно.</returns>
    public bool TryResearch(ResearchNodeSO node)
    {
        if (!CanResearch(node)) return false;

        // Списываем чертежи
        ResourceCost cost = new ResourceCost(ResourceType.Blueprints, node.BlueprintCost);
        EventHub.OnGlobalResourceSpendRequested?.Invoke(cost);

        // Устанавливаем состояние "исследовано"
        researchState.SetNodeState(node.name, ResearchNodeState.Researched);
        OnNodeStateChanged?.Invoke(node.name, ResearchNodeState.Researched);

        // Применяем эффекты исследования
        Debug.Log($"Applying research effects for: {node.NodeName}");
        ResearchEffectsManager.Instance?.ApplyResearchEffects(node);

        // Обновляем состояния зависимых узлов
        UpdateDependentNodes(node);

        Debug.Log($"Researched: {node.NodeName}");
        return true;
    }

    /// <summary>
    /// Обновляет состояния всех узлов, которые зависят от исследованного узла.
    /// </summary>
    /// <param name="researchedNode">Только что исследованный узел.</param>
    private void UpdateDependentNodes(ResearchNodeSO researchedNode)
    {
        // Находим все узлы, которые зависят от исследованного узла
        foreach (ResearchBranchSO branch in allBranches)
        {
            foreach (ResearchNodeSO node in branch.ResearchNodes)
            {
                if (node.PrerequisiteNodes != null &&
                    System.Array.Exists(node.PrerequisiteNodes, prereq => prereq.name == researchedNode.name))
                {
                    ResearchNodeState newState = CalculateNodeState(node);
                    ResearchNodeState oldState = researchState.GetNodeState(node.name);

                    if (oldState != newState && oldState != ResearchNodeState.Researched)
                    {
                        researchState.SetNodeState(node.name, newState);
                        OnNodeStateChanged?.Invoke(node.name, newState);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Сбрасывает весь прогресс исследований (для тестирования).
    /// Восстанавливает начальное состояние всех узлов.
    /// </summary>
    public void ResetResearchProgress()
    {
        researchState.ResetState();
        InitializeDefaultStates();
        Debug.Log("Research progress reset.");

        // Оповещаем об изменении всех состояний
        foreach (ResearchBranchSO branch in allBranches)
        {
            foreach (ResearchNodeSO node in branch.ResearchNodes)
            {
                OnNodeStateChanged?.Invoke(node.name, GetNodeState(node));
            }
        }
    }

    /// <summary>
    /// Обновляет состояния всех узлов исследований.
    /// Вызывается при изменении ресурсов или других условий, влияющих на доступность.
    /// </summary>
    public void UpdateAllNodesStates()
    {
        if (allBranches == null) return;

        foreach (ResearchBranchSO branch in allBranches)
        {
            if (branch.ResearchNodes == null) continue;

            foreach (ResearchNodeSO node in branch.ResearchNodes)
            {
                ResearchNodeState currentState = GetNodeState(node);
                ResearchNodeState calculatedState = CalculateNodeState(node);

                // Обновляем состояние если оно изменилось
                if (currentState != calculatedState && currentState != ResearchNodeState.Researched)
                {
                    researchState.SetNodeState(node.name, calculatedState);
                    OnNodeStateChanged?.Invoke(node.name, calculatedState);
                }
            }
        }
    }

    /// <summary>
    /// Возвращает массив всех вкладок исследований.
    /// </summary>
    /// <returns>Массив ResearchTabSO.</returns>
    public ResearchTabSO[] GetResearchTabs()
    {
        return researchTabs;
    }

    /// <summary>
    /// Возвращает ветки исследований, принадлежащие указанной вкладке.
    /// </summary>
    /// <param name="tabType">Тип вкладки.</param>
    /// <returns>Массив веток для этой вкладки.</returns>
    public ResearchBranchSO[] GetBranchesForTab(ResearchTabType tabType)
    {
        ResearchTabSO tab = System.Array.Find(researchTabs, t => t.TabType == tabType);
        return tab != null ? tab.BranchesInTab : new ResearchBranchSO[0];
    }

    /// <summary>
    /// Проверяет, доступен ли раздел исследований для просмотра и взаимодействия.
    /// </summary>
    /// <param name="tabType">Тип вкладки для проверки.</param>
    /// <returns>True если вкладка доступна.</returns>
    public bool IsTabAvailable(ResearchTabType tabType)
    {
        ResearchTabSO tab = System.Array.Find(researchTabs, t => t.TabType == tabType);
        if (tab == null) return false;

        // Здесь нужно добавить логику разблокировки разделов 
        return !tab.IsLocked;
    }
}