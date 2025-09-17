using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        startGameButton.onClick.AddListener(OnStartGameClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        // Убеждаемся, что время течет нормально
        Time.timeScale = 1f;
    }

    private void OnStartGameClicked()
    {
        Debug.Log("Start Game button clicked");
        // Загружаем стратегическую карту
        SceneLoaderManager.Instance.LoadStrategyMap();
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked");
        // TODO: Реализовать открытие настроек
    }

    private void OnExitClicked()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}