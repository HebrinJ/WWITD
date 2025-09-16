using UnityEngine;
using UnityEngine.UI;

public class UI_ConstructionButton : MonoBehaviour
{
    [SerializeField] private Button constructionButton;
    [SerializeField] private Image buttonIcon;
    [SerializeField] private Sprite constructionOnIcon;
    [SerializeField] private Sprite constructionOffIcon;

    private void Start()
    {
        constructionButton.onClick.AddListener(ToggleConstructionMode);
        EventHub.OnConstructionModeChanged += UpdateButtonIcon;
    }

    private void ToggleConstructionMode()
    {
        EventHub.OnConstructionModeToggled?.Invoke();
    }

    private void UpdateButtonIcon(bool isConstructionModeActive)
    {
        buttonIcon.sprite = isConstructionModeActive ? constructionOnIcon : constructionOffIcon;
    }

    private void OnDestroy()
    {
        EventHub.OnConstructionModeChanged -= UpdateButtonIcon;
    }
}
