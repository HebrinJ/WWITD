using System.Collections;
using UnityEngine;

/// <summary>
/// Менеджер, отвечающий за управление игровым процессом на тактическом уровне.
/// Координирует условия победы и поражения, обрабатывает урон по базе и запускает соответствующие события завершения уровня.
/// Является центральным узлом, принимающим окончательное решение о результате уровня.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Агрегирует ключевые системы уровня (штаб, волны) и реагирует на их события
/// для определения исхода уровня (победа/поражение). Только этот менеджер может инициировать завершение уровня.</para>
/// </remarks>
public class LevelManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Ссылка на компонент штаба (главного здания), который необходимо защищать.")]
    private Headquarters headquarters;

    [SerializeField]
    [Tooltip("Ссылка на менеджер локальных ресурсов уровня.")]
    private LocalResourceManager localResourceManager;

    /// <summary>
    /// Флаг, указывающий, что игра на уровне завершена (победа или поражение).
    /// Предотвращает множественную обработку условий завершения.
    /// </summary>
    private bool isGameOver = false;

    private void OnEnable()
    {
        // Подписываемся на событие достижения врагом базы
        EventHub.OnEnemyReachedBase += HandleEnemyReachedBase;

        // Подписываемся на событие уничтожения штаба
        if (headquarters != null)
        {
            headquarters.OnHeadquartersDestroyed += HandleGameOver;
        }

        // Подписываемся на событие завершения всех волн
        EventHub.OnAllWavesCompleted += HandleAllWavesCompleted;        
    }

    private void OnDisable()
    {
        EventHub.OnEnemyReachedBase -= HandleEnemyReachedBase;

        if (headquarters != null)
        {
            headquarters.OnHeadquartersDestroyed -= HandleGameOver;
        }

        EventHub.OnAllWavesCompleted -= HandleAllWavesCompleted;
    }

    private void Start()
    {
        Debug.Log($"[LevelManager] Уровень начался: {gameObject.scene.name}");
    }

    /// <summary>
    /// Обработчик события достижения врагом штаба. Наносит урон штабу на основе данных врага.
    /// </summary>
    /// <param name="enemy">Поведение врага, достигшего базы. Содержит данные для расчета урона.</param>
    private void HandleEnemyReachedBase(EnemyBehaviour enemy)
    {
        // Защита от обработки после завершения игры или если враг null
        if (isGameOver || enemy == null) return;

        // Наносим урон штабу, используя значение damageToBase из данных врага
        headquarters?.TakeDamage(enemy.Data.damageToBase);
    }

    /// <summary>
    /// Обработчик события уничтожения штаба. Запускает сценарий поражения.
    /// </summary>
    private void HandleGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("[LevelManager] Уровень провален! Штаб уничтожен.");
        // Оповещаем все системы о поражении
        EventHub.OnGameOver?.Invoke();

        // Через 3 секунды возвращаемся на карту (Временное решение)
        StartCoroutine(ReturnToStrategyMapAfterDelay(3f));
    }

    /// <summary>
    /// Обработчик события завершения всех волн. Запускает сценарий победы.
    /// </summary>
    private void HandleAllWavesCompleted()
    {
        if (isGameOver) return;

        Debug.Log("[LevelManager] Все волны пройдены! Уровень завершен успешно.");
        CompleteLevel(true); // Запускаем сценарий победы
    }

    /// <summary>
    /// Вспомогательная корутина для возврата на стратегическую карту через задержку.
    /// </summary>
    /// <param name="delay">Задержка в секундах перед возвратом.</param>
    /// <returns>IEnumerator для работы корутины.</returns>
    private IEnumerator ReturnToStrategyMapAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Используем менеджер сцен для возврата на карту
        SceneLoaderManager.Instance.LoadStrategyMap();
    }

    /// <summary>
    /// Основной метод для завершения уровня. Вызывается при победе или поражении.
    /// </summary>
    /// <param name="victory">true - засчитать победу, false - засчитать поражение.</param>
    public void CompleteLevel(bool victory)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (victory)
        {
            Debug.Log("[LevelManager] Уровень пройден успешно!");

            // Оповещаем через EventHub о завершении уровня
            string levelId = gameObject.scene.name; // или используйте специальный идентификатор
            EventHub.OnLevelCompleted?.Invoke(levelId);

            // Оповещаем все системы о победе
            EventHub.OnLevelComplete?.Invoke();
        }

        // Возвращаемся на карту через задержку
        StartCoroutine(ReturnToStrategyMapAfterDelay(3f));
    }
}