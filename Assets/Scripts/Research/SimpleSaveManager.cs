using UnityEngine;

public class SimpleSaveManager : MonoBehaviour
{
    public static SimpleSaveManager Instance { get; private set; }

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

    private void OnApplicationQuit()
    {
        SaveAllData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllData();
        }
    }

    public void SaveAllData()
    {
        SaveResearchState();
        PlayerPrefs.Save();
        Debug.Log("Game data saved");
    }

    public void LoadAllData()
    {
        LoadResearchState();
        Debug.Log("Game data loaded");
    }

    private void SaveResearchState()
    {
        if (researchState != null)
        {
            researchState.SaveState();
        }
    }

    private void LoadResearchState()
    {
        if (researchState != null)
        {
            researchState.LoadState();
        }
    }

    // Метод для сброса всех сохранений (для тестирования)
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