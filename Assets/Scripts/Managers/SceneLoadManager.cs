using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Синглтон-менеджер, отвечающий за асинхронную загрузку, выгрузку и переключение игровых сцен.
/// Координирует переходы между главным меню, стратегической картой и тактическими уровнями.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является центральным узлом управления сценами. Обеспечивает плавные переходы
/// между различными состояниями игры, синхронизируя загрузку сцен с изменением <see cref="GameState"/>.</para>
/// <para><b>Паттерн Singleton:</b> Реализован как синглтон с сохранением между сценами (<see cref="DontDestroyOnLoad"/>),
/// что гарантирует его доступность из любой точки игры и непрерывность процесса загрузки.</para>
/// <para><b>Асинхронные операции:</b> Все операции загрузки/выгрузки выполняются асинхронно через корутины для
/// предотвращения фризов и возможности отображения прогресса.</para>
/// </remarks>
public class SceneLoaderManager : MonoBehaviour
{
    /// <summary>
    /// Статическое свойство для глобального доступа к экземпляру менеджера.
    /// </summary>
    public static SceneLoaderManager Instance;

    [SerializeField]
    [Tooltip("Имя сцены стратегической карты кампании.")]
    private string strategyMapSceneName = "Map";

    [SerializeField]
    [Tooltip("Имя сцены главного меню.")]
    private string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// Имя текущей активной игровой сцены (уровня, карты или меню). Используется для её последующей выгрузки.
    /// </summary>
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

    /// <summary>
    /// Загружает главное меню при старте игры.
    /// </summary>
    private void Start()
    {
        LoadMainMenu();
    }

    /// <summary>
    /// Публичный метод для загрузки сцены главного меню.
    /// </summary>
    public void LoadMainMenu()
    {
        StartCoroutine(SwitchScene(mainMenuSceneName, GameState.MainMenu));
    }

    /// <summary>
    /// Публичный метод для загрузки сцены стратегической карты кампании.
    /// </summary>
    public void LoadStrategyMap()
    {
        StartCoroutine(SwitchScene(strategyMapSceneName, GameState.StrategyMap));
    }

    /// <summary>
    /// Публичный метод для загрузки конкретного тактического уровня.
    /// </summary>
    /// <param name="levelName">Имя сцены уровня для загрузки.</param>
    public void LoadTacticalLevel(string levelName)
    {
        StartCoroutine(SwitchScene(levelName, GameState.TacticalLevel));
    }

    /// <summary>
    /// Основная корутина для переключения между сценами. Выполняет последовательную выгрузку текущей сцены
    /// и загрузку новой, затем обновляет состояние игры.
    /// </summary>
    /// <param name="newSceneName">Имя сцены для загрузки.</param>
    /// <param name="targetState">Целевое состояние игры, соответствующее новой сцене.</param>
    /// <returns>IEnumerator для работы корутины.</returns>
    private IEnumerator SwitchScene(string newSceneName, GameState targetState)
    {
        Debug.Log($"[SceneLoader] Начинается загрузка сцены: {newSceneName}");

        // Выгружаем текущую активную сцену (если она есть)
        if (!string.IsNullOrEmpty(currentActiveScene))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentActiveScene);
            while (!unloadOp.isDone)
            {
                // Здесь можно обновлять UI прогресса выгрузки
                yield return null;
            }
            Debug.Log($"[SceneLoader] Сцена выгружена: {currentActiveScene}");
        }

        // Асинхронно загружаем новую сцену в additive mode (дополнительно к текущей, которой является "persistent")
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            // Здесь можно обновлять UI прогресса загрузки (loadOp.progress)
            yield return null;
        }

        // Делаем новую сцену активной
        Scene newScene = SceneManager.GetSceneByName(newSceneName);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
            currentActiveScene = newSceneName;
            Debug.Log($"[SceneLoader] Сцена загружена и активирована: {newSceneName}");
        }
        else
        {
            Debug.LogError($"[SceneLoader] Ошибка: Сцена '{newSceneName}' не была найдена или загружена некорректно.");
            yield break;
        }

        // Синхронизируем состояние игры с загруженной сценой
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(targetState);
        }
        else
        {
            Debug.LogWarning("[SceneLoader] GameStateManager не найден. Состояние игры не было обновлено.");
        }
    }
}