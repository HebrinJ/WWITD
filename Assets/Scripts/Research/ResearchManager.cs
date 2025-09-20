using System.Collections.Generic;
using UnityEngine;

public class ResearchManager : MonoBehaviour
{
    public static ResearchManager Instance { get; private set; }

    [SerializeField] private ResearchStateSO researchState;
    [SerializeField] private ResearchBranchSO[] allBranches; // ��� ����� ������������

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

        // ������ �� ������ ��������� ����� - ��� ��� � SO
        InitializeDefaultStates();
    }

    private void InitializeDefaultStates()
    {
        // �������������� ��������� �� ��������� ��� ���� �����
        foreach (ResearchBranchSO branch in allBranches)
        {
            foreach (ResearchNodeSO node in branch.ResearchNodes)
            {
                ResearchNodeState currentState = researchState.GetNodeState(node.name);

                // ���� ��������� ��� � SO, ������������� ���������
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

        // ���� ���� �� ����������, ��������� ��� ������� ���������
        if (state != ResearchNodeState.Researched)
        {
            state = CalculateNodeState(node);
            // ��������� ����������� ��������� (�� �� �������������� Researched!)
            if (researchState.GetNodeState(node.name) != ResearchNodeState.Researched)
            {
                researchState.SetNodeState(node.name, state);
            }
        }

        return state;
    }

    private ResearchNodeState CalculateNodeState(ResearchNodeSO node)
    {
        // ��������� prerequisites
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

        // ��������� �������
        int currentBlueprints = GlobalResourceManager.Instance?.GetResourceAmount(ResourceType.Blueprints) ?? 0;
        return currentBlueprints >= node.BlueprintCost ?
            ResearchNodeState.Available : ResearchNodeState.Available; // ����� �������� ��������� ������ "NoResources"
    }

    public bool CanResearch(ResearchNodeSO node)
    {
        return GetNodeState(node) == ResearchNodeState.Available;
    }

    public bool TryResearch(ResearchNodeSO node)
    {
        if (!CanResearch(node)) return false;

        // ��������� �������
        ResourceCost cost = new ResourceCost(ResourceType.Blueprints, node.BlueprintCost);
        EventHub.OnGlobalResourceSpendRequested?.Invoke(cost);

        // ������������� ��������� "�����������"
        researchState.SetNodeState(node.name, ResearchNodeState.Researched);
        OnNodeStateChanged?.Invoke(node.name, ResearchNodeState.Researched);

        // ��������� ��������� ��������� �����
        UpdateDependentNodes(node);

        Debug.Log($"Researched: {node.NodeName}");
        return true;
    }

    private void UpdateDependentNodes(ResearchNodeSO researchedNode)
    {
        // ������� ��� ����, ������� ������� �� �������������� ����
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

        // ��������� �� ��������� ���� ���������
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

                // ��������� ��������� ���� ��� ����������
                if (currentState != calculatedState && currentState != ResearchNodeState.Researched)
                {
                    researchState.SetNodeState(node.name, calculatedState);
                    OnNodeStateChanged?.Invoke(node.name, calculatedState);
                }
            }
        }
    }
}