using UnityEngine;

[CreateAssetMenu(fileName = "New Research Branch", menuName = "Research System/Research Branch")]
public class ResearchBranchSO : ScriptableObject
{
    public string BranchName;
    public ResearchNodeSO[] ResearchNodes; // ������ ���� ����� � ���� �����, � ������� ������������
}