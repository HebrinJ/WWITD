using UnityEngine;

[CreateAssetMenu(fileName = "ResearchNodeSO", menuName = "Scriptable Objects/ResearchNodeSO")]
public class ResearchNodeSO : ScriptableObject
{
    public string NodeName;
    [TextArea] public string Description;
    public ResearchNodeType NodeType;
    public int BlueprintCost;
    public Sprite Icon;

    // Пока оставим требования простыми. Позже усложним.
    public ResearchNodeSO[] PrerequisiteNodes; // Какие узлы должны быть исследованы ДО этого
}
