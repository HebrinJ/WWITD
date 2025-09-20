using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ResearchPanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ResearchBranchSO testBranch; // ���������� ���� Branch_Riflemen �� �������
    [SerializeField] private Transform branchesContainer; // Parent object ��� �����
    [SerializeField] private UI_ResearchNode researchNodePrefab;
    [SerializeField] private Image connectorLinePrefab;

    private void OnEnable()
    {
        // ��������: ��� �������� ������ ������ ������� �������� �����
        if (testBranch != null)
        {
            CreateBranchUI(testBranch);
        }
    }

    private void OnDisable()
    {
        // ������� ������ ��� ��������, ����� �� �������� ������ ��������
        foreach (Transform child in branchesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateBranchUI(ResearchBranchSO branchData)
    {
        // 1. ������� ��������� ��� ����� (������������ ��� ��������������)
        GameObject branchContainer = new GameObject(branchData.BranchName, typeof(VerticalLayoutGroup));
        var layout = branchContainer.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 150;

        branchContainer.transform.SetParent(branchesContainer, false);

        // 2. ��� ������� ���� � �����...
        for (int i = 0; i < branchData.ResearchNodes.Length; i++)
        {
            ResearchNodeSO node = branchData.ResearchNodes[i];

            // 3. ������� UI ������� ��� ����
            UI_ResearchNode nodeUI = Instantiate(researchNodePrefab, branchContainer.transform);
            nodeUI.Initialize(node); // ���� ����� �� �������� � ������� UI_ResearchNode

            // 4. (�����) ����� ����� �������� ����� ����� ������
            // if (i > 0) { DrawConnector(...); }
        }
    }

    // ���� ����� ����� ���������� �� ������ � UI
    public void OnResearchButtonClicked(ResearchNodeSO nodeData)
    {
        Debug.Log($"Button clicked for research: {nodeData.NodeName}");
        // ���� ������ ��������. � ��������� ����� ������� ����� ��������� ������������.
    }
}