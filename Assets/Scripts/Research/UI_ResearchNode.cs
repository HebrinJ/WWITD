using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ResearchNode : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button researchButton;
    [SerializeField] private Image backgroundImage;

    [Header("State Colors")]
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color availableColor = Color.yellow;
    [SerializeField] private Color researchedColor = Color.green;
    [SerializeField] private Color unavailableColor = Color.red; // ������� ���� ��� "��������, �� ��� ��������"

    private ResearchNodeSO nodeData;

    public void Initialize(ResearchNodeSO data)
    {
        nodeData = data;
        nameText.text = data.NodeName;
        costText.text = data.BlueprintCost.ToString();

        researchButton.onClick.AddListener(OnButtonClick);

        // ������������� �� ������� ��������� ������ ����
        ResearchManager.OnNodeStateChanged += HandleAnyNodeStateChanged;

        // �������������� ����������
        UpdateVisuals();
    }

    private void OnDestroy()
    {
        ResearchManager.OnNodeStateChanged -= HandleAnyNodeStateChanged;
    }

    private void HandleAnyNodeStateChanged(string changedNodeName, ResearchNodeState newState)
    {
        // ��� ��������� ������ ���� ��������� ���� ������
        // �������� ��� ����������� � ���������� ���������
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (ResearchManager.Instance == null || nodeData == null) return;

        ResearchNodeState state = ResearchManager.Instance.GetNodeState(nodeData);

        switch (state)
        {
            case ResearchNodeState.Locked:
                backgroundImage.color = lockedColor;
                researchButton.interactable = false;
                costText.text = "Locked";
                break;

            case ResearchNodeState.Available:
                // ���������, ������� �� �������� ��� ����������� ������� �����
                int currentBlueprints = GlobalResourceManager.Instance.GetResourceAmount(ResourceType.Blueprints);
                if (currentBlueprints >= nodeData.BlueprintCost)
                {
                    backgroundImage.color = availableColor;
                    researchButton.interactable = true;
                    costText.text = nodeData.BlueprintCost.ToString();
                }
                else
                {
                    backgroundImage.color = unavailableColor;
                    researchButton.interactable = false;
                    costText.text = $"{nodeData.BlueprintCost} (Need {nodeData.BlueprintCost - currentBlueprints})";
                }
                break;

            case ResearchNodeState.Researched:
                backgroundImage.color = researchedColor;
                researchButton.interactable = false;
                costText.text = "Researched";
                break;
        }
    }

    private void OnButtonClick()
    {
        bool success = ResearchManager.Instance.TryResearch(nodeData);
        if (!success)
        {
            Debug.LogWarning($"Cannot research {nodeData.NodeName}");
            // ����� �������� �������� ��� ���� ������
        }
    }
}