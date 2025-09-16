using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLevelLoader : MonoBehaviour
{
    [SerializeField] private string levelToLoad = "Level01";
    [SerializeField] private float delayBeforeLoad = 1.0f;

    private void Start()
    {
        // ∆дем немного чтобы все менеджеры инициализировались
        Invoke(nameof(LoadLevel), delayBeforeLoad);
    }

    private void LoadLevel()
    {
        SceneLoaderManager.Instance.LoadTacticalLevel(levelToLoad);
        Debug.Log($"Automatically loading level: {levelToLoad}");
    }
}