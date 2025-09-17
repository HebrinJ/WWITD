using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoaderManager : MonoBehaviour
{
    public static SceneLoaderManager Instance;

    [SerializeField] private string strategyMapSceneName = "Map";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private string currentActiveScene = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadMainMenu();
    }

    public void LoadMainMenu()
    {
        StartCoroutine(SwitchScene(mainMenuSceneName, GameState.MainMenu));
    }

    public void LoadStrategyMap()
    {
        StartCoroutine(SwitchScene(strategyMapSceneName, GameState.StrategyMap));
    }

    public void LoadTacticalLevel(string levelName)
    {
        StartCoroutine(SwitchScene(levelName, GameState.TacticalLevel));
    }

    private IEnumerator SwitchScene(string newSceneName, GameState targetState)
    {
        Debug.Log($"Loading scene: {newSceneName}");

        // Выгружаем текущую сцену (если есть)
        if (!string.IsNullOrEmpty(currentActiveScene))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentActiveScene);
            while (!unloadOp.isDone)
            {
                yield return null;
            }
            Debug.Log($"Unloaded scene: {currentActiveScene}");
        }

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            // Можно добавить progress bar здесь
            yield return null;
        }

        // Активируем новую сцену
        Scene newScene = SceneManager.GetSceneByName(newSceneName);
        SceneManager.SetActiveScene(newScene);
        currentActiveScene = newSceneName;

        Debug.Log($"Scene loaded and activated: {newSceneName}");

        // Меняем состояние игры
        GameStateManager.Instance.SetState(targetState);
    }

    // Метод для возврата в главное меню (просто выгружаем текущую сцену)
    public void ReturnToMainMenu()
    {
        if (!string.IsNullOrEmpty(currentActiveScene))
        {
            StartCoroutine(UnloadCurrentScene());
            GameStateManager.Instance.SetState(GameState.MainMenu);
        }
    }

    private IEnumerator UnloadCurrentScene()
    {
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentActiveScene);
        while (!unloadOp.isDone)
        {
            yield return null;
        }
        currentActiveScene = "";
    }
}
