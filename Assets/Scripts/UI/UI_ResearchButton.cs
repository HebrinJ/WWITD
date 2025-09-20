using UnityEngine;
using UnityEngine.UI;

public class UI_ResearchButton : MonoBehaviour
{
    [SerializeField] private Button researchButton;
    
    private void Start()
    {
        researchButton.onClick.AddListener(OnResearchButtonClick);        
    }

    private void OnResearchButtonClick()
    {
        if (GameStateManager.Instance.CurrentState == GameState.MainMenu)
        {
            Debug.LogWarning("Research not available in Main Menu");
            return;
        }

        UI_Manager.Instance.ToggleResearchPanel();
    }
}