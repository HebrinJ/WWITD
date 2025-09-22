using UnityEngine;

/// <summary>
/// Менеджер, отвечающий за визуальное включение и отключение режима строительства на тактическом уровне.
/// Управляет активностью всех игровых объектов-плейсхолдеров, делая их видимыми или скрытыми в зависимости от состояния режима.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является связующим звеном между UI (кнопкой "Режим строительства") и визуальным представлением игрового поля.
/// Не отвечает за логику строительства (это задача <see cref="BuildManager"/>), только за визуальную индикацию доступных мест.</para>
/// <para><b>Визуальная обратная связь:</b> Критически важен для юзабилити, так как четко показывает игроку, где можно строить, когда режим активен.</para>
/// </remarks>
public class ConstructionModeManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Массив всех игровых объектов, являющихся плейсхолдерами для оборонительных башен.")]
    private GameObject[] constructionPlaceholders;

    [SerializeField]
    [Tooltip("Массив всех игровых объектов, являющихся плейсхолдерами для инженерных сооружений (колючая проволока, ежи).")]
    private GameObject[] engineeringPlaceholders;

    [SerializeField]
    [Tooltip("Массив всех игровых объектов, являющихся плейсхолдерами для мостов (ключевые точки).")]
    private GameObject[] bridgePlaceholders;

    /// <summary>
    /// Флаг, отражающий текущее состояние режима строительства.
    /// true - режим активен, плейсхолдеры видны.
    /// false - режим неактивен, плейсхолдеры скрыты.
    /// </summary>
    private bool isConstructionModeActive = false;

    private void OnEnable()
    {
        // Подписываемся на событие кнопки "Режим строительства"
        EventHub.OnConstructionModeToggled += ToggleConstructionMode;

        // Гарантируем, что при старте уровня плейсхолдеры не видны
        SetAllPlaceholdersActive(false);
    }

    private void OnDisable()
    {
        EventHub.OnConstructionModeToggled -= ToggleConstructionMode;
    }

    /// <summary>
    /// Основной метод переключения состояния режима строительства.
    /// Вызывается по событию из UI. Инвертирует текущее состояние и обновляет видимость всех плейсхолдеров.
    /// </summary>
    public void ToggleConstructionMode()
    {
        // Инвертируем текущее состояние
        isConstructionModeActive = !isConstructionModeActive;

        // Применяем новое состояние ко всем плейсхолдерам
        SetAllPlaceholdersActive(isConstructionModeActive);

        Debug.Log($"[ConstructionMode] Режим строительства: {isConstructionModeActive}");

        // Оповещаем все заинтересованные системы о изменении состояния режима
        EventHub.OnConstructionModeChanged?.Invoke(isConstructionModeActive);
    }

    /// <summary>
    /// Вспомогательный метод для управления активностью всех категорий плейсхолдеров одновременно.
    /// </summary>
    /// <param name="isActive">true - активировать все плейсхолдеры, false - деактивировать.</param>
    private void SetAllPlaceholdersActive(bool isActive)
    {
        SetPlaceholdersActive(constructionPlaceholders, isActive);
        SetPlaceholdersActive(engineeringPlaceholders, isActive);
        SetPlaceholdersActive(bridgePlaceholders, isActive);
    }

    /// <summary>
    /// Вспомогательный метод для управления активностью конкретного массива плейсхолдеров.
    /// Выполняет проверку на null для каждого элемента массива перед попыткой изменения его активности.
    /// </summary>
    /// <param name="placeholders">Массив игровых объектов-плейсхолдеров.</param>
    /// <param name="isActive">Целевое состояние активности.</param>
    private void SetPlaceholdersActive(GameObject[] placeholders, bool isActive)
    {
        if (placeholders == null) return;

        foreach (var placeholder in placeholders)
        {
            if (placeholder != null)
            {
                placeholder.SetActive(isActive);
            }
        }
    }
}