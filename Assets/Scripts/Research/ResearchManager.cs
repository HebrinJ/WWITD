using System.Collections.Generic;
using UnityEngine;

public class ResearchManager : MonoBehaviour
{
    public static ResearchManager Instance { get; private set; }

    [SerializeField] private ResearchStateSO researchState;
    [SerializeField] private ResearchBranchSO[] allBranches; // Все ветки исследований

    public static event System.Action<string, ResearchNodeState> OnNodeStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Теперь не грузим состояние здесь - оно уже в SO
        InitializeDefaultStates();
    }

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

    public ResearchNodeState GetNodeState(ResearchNodeSO node)
    {
        if (node == null) return ResearchNodeState.Locked;

        ResearchNodeState state = researchState.GetNodeState(node.name);

        // Если узел не исследован, вычисляем его текущее состояние
        if (state != ResearchNodeState.Researched)
        {
            state = CalculateNodeState(node);
            // Сохраняем вычисленное состояние (но не перезаписываем Researched!)
            if (researchState.GetNodeState(node.name) != ResearchNodeState.Researched)
            {
                researchState.SetNodeState(node.name, state);
            }
        }

        return state;
    }

    private ResearchNodeState CalculateNodeState(ResearchNodeSO node)
    {
        // Проверяем prerequisites
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

        // Проверяем ресурсы
        int currentBlueprints = GlobalResourceManager.Instance?.GetResourceAmount(ResourceType.Blueprints) ?? 0;
        return currentBlueprints >= node.BlueprintCost ?
            ResearchNodeState.Available : ResearchNodeState.Available; // Можно добавить отдельный статус "NoResources"
    }

    public bool CanResearch(ResearchNodeSO node)
    {
        return GetNodeState(node) == ResearchNodeState.Available;
    }

    public bool TryResearch(ResearchNodeSO node)
    {
        if (!CanResearch(node)) return false;

        // Списываем чертежи
        ResourceCost cost = new ResourceCost(ResourceType.Blueprints, node.BlueprintCost);
        EventHub.OnGlobalResourceSpendRequested?.Invoke(cost);

        // Устанавливаем состояние "исследовано"
        researchState.SetNodeState(node.name, ResearchNodeState.Researched);
        OnNodeStateChanged?.Invoke(node.name, ResearchNodeState.Researched);

        // Обновляем состояния зависимых узлов
        UpdateDependentNodes(node);

        Debug.Log($"Researched: {node.NodeName}");
        return true;
    }

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
}