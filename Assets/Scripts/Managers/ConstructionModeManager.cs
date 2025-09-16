using UnityEngine;

public class ConstructionModeManager : MonoBehaviour
{
    [SerializeField] private GameObject[] constructionPlaceholders; // ��� ����� ��� �������������
    [SerializeField] private GameObject[] engineeringPlaceholders;  // ���������� �����
    [SerializeField] private GameObject[] bridgePlaceholders;       // ����� ��� ������

    private bool isConstructionModeActive = false;

    private void OnEnable()
    {
        // ������������� �� ������� ������ "����� �������������"
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

        // ��������� UI � ����� ������
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