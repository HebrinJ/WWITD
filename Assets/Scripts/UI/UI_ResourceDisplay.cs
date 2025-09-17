using UnityEngine;
using TMPro;

public class UI_ResourceDisplay : MonoBehaviour
{
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private bool isGlobalResource;

    private void OnEnable()
    {
        // Подписываемся на нужное событие в зависимости от типа ресурса
        if (isGlobalResource)
        {
            EventHub.OnGlobalResourceChanged += HandleResourceChanged;
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

    private void Start()
    {
        //TODO: Заправшивать ресурсы из правильного менеджера
    }
}