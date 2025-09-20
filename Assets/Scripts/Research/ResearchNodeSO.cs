using UnityEngine;

[CreateAssetMenu(fileName = "ResearchNodeSO", menuName = "Scriptable Objects/ResearchNodeSO")]
public class ResearchNodeSO : ScriptableObject
{
    public string NodeName;
    [TextArea] public string Description;
    public ResearchNodeType NodeType;
    public int BlueprintCost;
    public Sprite Icon;

    // ���� ������� ���������� ��������. ����� ��������.
    public ResearchNodeSO[] PrerequisiteNodes; // ����� ���� ������ ���� ����������� �� �����
}
