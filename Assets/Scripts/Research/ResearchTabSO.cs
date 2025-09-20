using UnityEngine;

[CreateAssetMenu(fileName = "New Research Tab", menuName = "Research System/Research Tab")]
public class ResearchTabSO : ScriptableObject
{
    public ResearchTabType TabType;
    public string TabName;
    public Sprite TabIcon;
    public ResearchBranchSO[] BranchesInTab; // ¬етки, которые относ€тс€ к этому разделу
    public bool IsLocked = false; // Ѕудет ли раздел изначально заблокирован
}