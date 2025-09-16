using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnPlayClicked()
    {
        // Переходим к стратегической карте
        SceneLoaderManager.Instance.LoadStrategyMap();
        GameStateManager.Instance.SetState(GameState.StrategyMap);
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings clicked");
        // Здесь будет открытие настроек
    }

    private void OnExitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}