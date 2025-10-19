using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI-компонент кнопки уровня на стратегической карте.
/// Отображает состояние уровня и обрабатывает взаимодействие игрока.
/// Предназначен для ручного размещения на сцене геймдизайнером.
/// </summary>
/// <remarks>
/// <para><b>Настройка в инспекторе:</b></para>
/// <list type="number">
/// <item><description>Разместите кнопку на стратегической карте</description></item>
/// <item><description>Назначьте LevelId соответствующий LevelDataSO</description></item>
/// <item><description>Настройте UI компоненты (изображение, текст и т.д.)</description></item>
/// </list>
/// </remarks>
public class UI_LevelButton : MonoBehaviour
{
    [Header("Level Configuration")]
    /// <summary>
    /// Идентификатор уровня, соответствующий LevelDataSO.
    /// Должен совпадать с levelId в ScriptableObject уровня.
    /// </summary>
    [Tooltip("Идентификатор уровня (должен совпадать с LevelDataSO.levelId).")]
    [SerializeField] private string levelId;

    [Header("UI Components")]
    /// <summary>
    /// Компонент Button для обработки нажатий.
    /// </summary>
    [Tooltip("Компонент Button этой кнопки уровня.")]
    [SerializeField] private Button button;

    /// <summary>
    /// Изображение иконки уровня.
    /// </summary>
    [Tooltip("Изображение иконки уровня.")]
    [SerializeField] private Image iconImage;

    /// <summary>
    /// Текст названия уровня.
    /// </summary>
    [Tooltip("Текст названия уровня.")]
    [SerializeField] private TextMeshProUGUI nameText;

    /// <summary>
    /// Оверлей заблокированного уровня.
    /// </summary>
    [Tooltip("GameObject оверлея заблокированного уровня.")]
    [SerializeField] private GameObject lockedOverlay;

    /// <summary>
    /// Оверлей пройденного уровня.
    /// </summary>
    [Tooltip("GameObject оверлея пройденного уровня.")]
    [SerializeField] private GameObject completedOverlay;

    /// <summary>
    /// Индикатор выбранного уровня.
    /// </summary>
    [Tooltip("Индикатор выбранного уровня.")]
    [SerializeField] private GameObject selectedIndicator;

    [Header("Visual States")]
    /// <summary>
    /// Цвет доступного уровня.
    /// </summary>
    [Tooltip("Цвет доступного уровня.")]
    [SerializeField] private Color availableColor = Color.white;

    /// <summary>
    /// Цвет заблокированного уровня.
    /// </summary>
    [Tooltip("Цвет заблокированного уровня.")]
    [SerializeField] private Color lockedColor = Color.gray;

    /// <summary>
    /// Цвет пройденного уровня.
    /// </summary>
    [Tooltip("Цвет пройденного уровня.")]
    [SerializeField] private Color completedColor = Color.green;

    /// <summary>
    /// Данные уровня, связанные с этой кнопкой.
    /// </summary>
    public LevelDataSO LevelData { get; private set; }

    /// <summary>
    /// Идентификатор уровня этой кнопки.
    /// </summary>
    public string LevelId => levelId;

    /// <summary>
    /// Инициализирует кнопку уровня.
    /// Вызывается UI_StrategyMap при старте.
    /// </summary>
    /// <param name="levelData">Данные уровня.</param>
    /// <param name="onClickCallback">Callback при нажатии на кнопку.</param>
    public void Initialize(LevelDataSO levelData, System.Action<LevelDataSO> onClickCallback)
    {
        LevelData = levelData;

        // Проверяем соответствие идентификаторов
        if (levelData.levelId != levelId)
        {
            Debug.LogWarning($"[UI_LevelButton] Несоответствие идентификаторов: кнопка '{levelId}', данные '{levelData.levelId}'");
        }

        // Настраиваем внешний вид
        if (nameText != null)
        {
            nameText.text = levelData.displayName;
        }

        if (iconImage != null && levelData.thumbnail != null)
        {
            iconImage.sprite = levelData.thumbnail;
        }

        // Назначаем обработчик
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke(levelData));
        }

        Debug.Log($"[UI_LevelButton] Инициализирована кнопка уровня: {levelData.displayName}");
    }

    /// <summary>
    /// Обновляет визуальное состояние кнопки в зависимости от состояния уровня.
    /// </summary>
    /// <param name="state">Текущее состояние уровня.</param>
    public void UpdateVisualState(LevelState state)
    {
        if (button != null)
        {
            button.interactable = state != LevelState.Locked;
        }

        // Обновляем оверлеи
        if (lockedOverlay != null)
            lockedOverlay.SetActive(state == LevelState.Locked);

        if (completedOverlay != null)
            completedOverlay.SetActive(state == LevelState.Completed);

        // Обновляем цвет
        if (iconImage != null)
        {
            iconImage.color = state switch
            {
                LevelState.Locked => lockedColor,
                LevelState.Available => availableColor,
                LevelState.Completed => completedColor,
                _ => availableColor
            };
        }

        Debug.Log($"[UI_LevelButton] Обновлено состояние кнопки {LevelId}: {state}");
    }

    /// <summary>
    /// Устанавливает или снимает выделение с кнопки.
    /// </summary>
    /// <param name="isSelected">true - выделить кнопку, false - снять выделение.</param>
    public void SetSelected(bool isSelected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(isSelected);
    }

    /// <summary>
    /// Вспомогательный метод для проверки корректности настройки в инспекторе.
    /// Вызывается в редакторе Unity.
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogWarning($"[UI_LevelButton] Не назначен levelId на {gameObject.name}", this);
        }

        if (button == null)
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"[UI_LevelButton] Не найден компонент Button на {gameObject.name}", this);
            }
        }
    }
}