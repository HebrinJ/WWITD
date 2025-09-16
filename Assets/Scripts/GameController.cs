using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController instance;

    public static GameController Instance
    {
        get
        {            
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameController>();

                if (instance == null)
                {
                    GameObject gameObject = new GameObject("GameController");
                    instance = gameObject.AddComponent<GameController>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("GameController initialized");
    }
}
