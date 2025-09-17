using TMPro;
using UnityEngine;

public class UI_ResourceDisplay : MonoBehaviour
{
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private bool isGlobalResource; // ����������, ����� �������� ������������

    private void OnEnable()
    {
        // ������������� �� ���������� ������� � ����������� �� ���� �������
        if (isGlobalResource)
        {
            EventHub.OnGlobalResourceChanged += HandleResourceChanged;
            UpdateGlobalResourceDisplay();
        }
        else
        {
            EventHub.OnLocalResourceChanged += HandleResourceChanged;
        }
    }

    private void OnDisable()
    {
        if (isGlobalResource)
        {
            EventHub.OnGlobalResourceChanged -= HandleResourceChanged;
        }
        else
        {
            EventHub.OnLocalResourceChanged -= HandleResourceChanged;
        }
    }

    private void HandleResourceChanged(ResourceType type, int amount)
    {
        if (type == resourceType)
        {
            amountText.text = amount.ToString();
        }
    }

    private void UpdateGlobalResourceDisplay()
    {
        if (isGlobalResource && GlobalResourceManager.Instance != null)
        {
            int amount = GlobalResourceManager.Instance.GetResourceAmount(resourceType);
            amountText.text = amount.ToString();
        }
    }

    // ��������� ����� ��� �������� ����������
    public void UpdateResourceDisplay(ResourceType type, int amount)
    {
        if (type == resourceType)
        {
            amountText.text = amount.ToString();
        }
    }
}