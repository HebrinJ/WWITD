using UnityEngine;

/// <summary>
/// Данные отдельного уровня на стратегической карте.
/// Определяет свойства уровня, его доступность и связи с другими уровнями.
/// </summary>
/// <remarks>
/// <para><b>Использование:</b> Создавайте ScriptableObject для каждого уровня в игре
/// и настраивайте зависимости открытия в инспекторе Unity.</para>
/// </remarks>
[CreateAssetMenu(fileName = "Level_", menuName = "Iron Tide/Level Data")]
public class LevelDataSO : ScriptableObject
{
    /// <summary>
    /// Уникальный идентификатор уровня.
    /// Должен соответствовать имени сцены уровня.
    /// </summary>
    [Tooltip("Уникальный идентификатор уровня (должен совпадать с именем сцены).")]
    public string levelId;

    /// <summary>
    /// Отображаемое название уровня в UI.
    /// </summary>
    [Tooltip("Отображаемое название уровня в интерфейсе.")]
    public string displayName;

    /// <summary>
    /// Описание уровня для UI.
    /// </summary>
    [Tooltip("Описание уровня, отображаемое в интерфейсе.")]
    [TextArea(2, 4)]
    public string description;

    /// <summary>
    /// Миниатюра уровня для UI.
    /// </summary>
    [Tooltip("Миниатюра уровня для отображения на карте.")]
    public Sprite thumbnail;

    /// <summary>
    /// Награда за прохождение уровня в чертежах.
    /// </summary>
    [Tooltip("Количество чертежей, выдаваемых за прохождение уровня.")]
    public int blueprintReward = 10;

    /// <summary>
    /// Уровни, которые должны быть пройдены для открытия данного уровня.
    /// Если массив пустой - уровень доступен с начала игры.
    /// </summary>
    [Tooltip("Уровни, необходимые для открытия данного уровня. Оставьте пустым для стартового уровня.")]
    public LevelDataSO[] requiredLevels;

    /// <summary>
    /// Уровни, которые открываются после прохождения данного уровня.
    /// </summary>
    [Tooltip("Уровни, которые становятся доступными после прохождения данного уровня.")]
    public LevelDataSO[] unlockedLevels;

    /// <summary>
    /// Позиция уровня на стратегической карте.
    /// </summary>
    [Tooltip("Позиция уровня на стратегической карте в относительных координатах.")]
    public Vector2 mapPosition;

    /// <summary>
    /// Проверяет, доступен ли уровень для выбора игроком.
    /// </summary>
    /// <param name="saveManager">Менеджер сохранения для проверки прогресса.</param>
    /// <returns>true - если уровень доступен, false - если заблокирован.</returns>
    public bool IsLevelAvailable(SaveManager saveManager)
    {
        // Если нет требований - уровень доступен
        if (requiredLevels == null || requiredLevels.Length == 0)
            return true;

        // Проверяем, пройдены ли все требуемые уровни
        foreach (var requiredLevel in requiredLevels)
        {
            if (requiredLevel != null && !saveManager.IsLevelCompleted(requiredLevel.levelId))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Получает состояние уровня для отображения в UI.
    /// </summary>
    /// <param name="saveManager">Менеджер сохранения для проверки прогресса.</param>
    /// <returns>Состояние уровня.</returns>
    public LevelState GetLevelState(SaveManager saveManager)
    {
        if (saveManager.IsLevelCompleted(levelId))
            return LevelState.Completed;

        if (IsLevelAvailable(saveManager))
            return LevelState.Available;

        return LevelState.Locked;
    }
}