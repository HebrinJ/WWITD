using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController instance;

    [SerializeField] private Headquarters headquarters;

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

    // Текущее состояние игры. Пока просто флаг.
    private bool isGameOver = false;

    private void OnEnable()
    {
        EventHub.OnEnemyReachedBase += HandleEnemyReachedBase;

        if (headquarters != null)
        {
            headquarters.OnHeadquartersDestroyed += HandleGameOver;
        }
    }

    private void OnDisable()
    {
        EventHub.OnEnemyReachedBase -= HandleEnemyReachedBase;
        if (headquarters != null)
        {
            headquarters.OnHeadquartersDestroyed -= HandleGameOver;
        }
    }

    private void HandleEnemyReachedBase(EnemyBehaviour enemy)
    {
        if (enemy == null || enemy.Data == null) return;
        if (isGameOver) return; // Если игра уже окончена, игнорируем урон.

        headquarters?.TakeDamage(enemy.Data.damageToBase);
    }

    private void HandleGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("GameManager: Game Over condition met!");

        // Останавливаем игровой процесс.
        Time.timeScale = 0f;

        EventHub.OnGameOver?.Invoke();
    }
}
