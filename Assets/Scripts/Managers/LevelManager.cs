using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Headquarters headquarters;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private LocalResourceManager localResourceManager;

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

    private void Start()
    {
        Debug.Log("Level started: " + gameObject.scene.name);
    }

    private void HandleEnemyReachedBase(EnemyBehaviour enemy)
    {
        if (isGameOver || enemy == null) return;
        headquarters?.TakeDamage(enemy.Data.damageToBase);
    }

    private void HandleGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Level failed!");
        EventHub.OnGameOver?.Invoke();

        // Через 3 секунды возвращаемся на карту (Переделать переход по нажатию кнопки в UI)
        StartCoroutine(ReturnToStrategyMapAfterDelay(3f));
    }

    private IEnumerator ReturnToStrategyMapAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneLoaderManager.Instance.LoadStrategyMap();
    }

    public void CompleteLevel(bool victory)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (victory)
        {
            Debug.Log("Level completed!");
            EventHub.OnLevelComplete?.Invoke();
        }

        StartCoroutine(ReturnToStrategyMapAfterDelay(3f));
    }
}