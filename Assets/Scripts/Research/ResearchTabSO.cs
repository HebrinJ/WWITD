using UnityEngine;

[CreateAssetMenu(fileName = "New Research Tab", menuName = "Research System/Research Tab")]
public class ResearchTabSO : ScriptableObject
{
    public ResearchTabType TabType;
    public string TabName;
    public Sprite TabIcon;
    public ResearchBranchSO[] BranchesInTab; // �����, ������� ��������� � ����� �������
    public bool IsLocked = false; // ����� �� ������ ���������� ������������
}