using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ResourceDisplay : MonoBehaviour
{
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private ResourceManager resourceManager;

    private void OnEnable()
    {
        EventHub.OnResourceAmountChanged += HandleResourceChanged;
    }

    private void OnDisable()
    {
        EventHub.OnResourceAmountChanged -= HandleResourceChanged;
    }

    private void HandleResourceChanged(ResourceType type, int amount)
    {
        if (type == resourceType)
        {
            amountText.text = amount.ToString();
        }
    }

    private void Start()
    {
        if (resourceManager != null)
        {
            amountText.text = resourceManager.GetResourceAmount(resourceType).ToString();
        }
    }
}