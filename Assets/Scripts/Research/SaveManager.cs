using UnityEngine;

/// <summary>
/// Центральный менеджер сохранения и загрузки игрового прогресса.
/// Реализует шаблон Singleton и сохраняется между сценами (DontDestroyOnLoad).
/// Координирует сохранение состояния всех систем игры (исследования, настройки, прогресс уровней).
/// Автоматически сохраняет данные при выходе из игры и приостановке приложения.
/// </summary>
public class SaveManager : MonoBehaviour
{
    /// <summary>
    /// Глобальная точка доступа к экземпляру SaveManager.
    /// Гарантирует, что в игре существует только один экземпляр менеджера сохранений.
    /// </summary>
    public static SaveManager Instance { get; private set; }

    /// <summary>
    /// Ссылка на ScriptableObject, содержащий состояние исследований игрока.
    /// Управляет сохранением и загрузкой прогресса в древе технологий.
    /// Должен быть назначен в инспекторе Unity.
    /// </summary>
    [Tooltip("ScriptableObject для сохранения состояния исследований. Назначьте в инспекторе.")]
    [SerializeField] private ResearchStateSO researchState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Загружаем данные сразу при старте
        LoadAllData();
    }

    /// <summary>
    /// Автоматически сохраняет все данные при завершении работы приложения.
    /// Вызывается системой Unity при закрытии игры.
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveAllData();
    }

    /// <summary>
    /// Автоматически сохраняет все данные при приостановке приложения.
    /// Особенно важно для мобильных платформ, когда игрок сворачивает приложение.
    /// </summary>
    /// <param name="pauseStatus">True если приложение приостановлено, false если возобновлено.</param>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllData();
        }
    }

    /// <summary>
    /// Сохраняет все игровые данные в постоянное хранилище.
    /// Вызывает методы сохранения для всех подсистем (исследования, настройки и т.д.).
    /// Принудительно записывает данные в PlayerPrefs.
    /// </summary>
    public void SaveAllData()
    {
        SaveResearchState();
        PlayerPrefs.Save();
        Debug.Log("Game data saved");
    }

    /// <summary>
    /// Загружает все игровые данные из постоянного хранилища.
    /// Восстанавливает состояние всех подсистем игры.
    /// </summary>
    public void LoadAllData()
    {
        LoadResearchState();
        Debug.Log("Game data loaded");
    }

    /// <summary>
    /// Сохраняет состояние исследований через связанный ResearchStateSO.
    /// </summary>
    private void SaveResearchState()
    {
        if (researchState != null)
        {
            researchState.SaveState();
        }
    }

    /// <summary>
    /// Загружает состояние исследований через связанный ResearchStateSO.
    /// </summary>
    private void LoadResearchState()
    {
        if (researchState != null)
        {
            researchState.LoadState();
        }
    }

    /// <summary>
    /// Сбрасывает весь игровой прогресс (только для тестирования).
    /// Удаляет все сохраненные данные и сбрасывает состояние исследований.
    /// ВНИМАНИЕ: Этот метод должен быть удален или защищен в production-версии.
    /// </summary>
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        if (researchState != null)
        {
            researchState.ResetState();
        }
        Debug.Log("All progress reset");
    }
}