using UnityEngine;

public class ConstructionModeManager : MonoBehaviour
{
    [SerializeField] private GameObject[] constructionPlaceholders; // Все места для строительства
    [SerializeField] private GameObject[] engineeringPlaceholders;  // Инженерные места
    [SerializeField] private GameObject[] bridgePlaceholders;       // Места для мостов

    private bool isConstructionModeActive = false;

    private void OnEnable()
    {
        // Подписываемся на событие кнопки "Режим строительства"
        EventHub.OnConstructionModeToggled += ToggleConstructionMode;

        SetAllPlaceholdersActive(false);
    }

    private void OnDisable()
    {
        EventHub.OnConstructionModeToggled -= ToggleConstructionMode;
    }

    public void ToggleConstructionMode()
    {
        isConstructionModeActive = !isConstructionModeActive;

        SetAllPlaceholdersActive(isConstructionModeActive);

        Debug.Log($"Construction mode: {isConstructionModeActive}");

        // Оповещаем UI о смене режима
        EventHub.OnConstructionModeChanged?.Invoke(isConstructionModeActive);
    }

    private void SetAllPlaceholdersActive(bool isActive)
    {
        SetPlaceholdersActive(constructionPlaceholders, isActive);
        SetPlaceholdersActive(engineeringPlaceholders, isActive);
        SetPlaceholdersActive(bridgePlaceholders, isActive);
    }

    private void SetPlaceholdersActive(GameObject[] placeholders, bool isActive)
    {
        if (placeholders == null) return;

        foreach (var placeholder in placeholders)
        {
            if (placeholder != null)
            {
                placeholder.SetActive(isActive);
            }
        }
    }
}