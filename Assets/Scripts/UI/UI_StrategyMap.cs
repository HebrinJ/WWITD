using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-компонент стратегической карты кампании.
/// Предоставляет интерфейс для выбора уровней, запуска тактических сражений и доступа к исследованиям.
/// Отображает прогресс игрока и доступные для прохождения уровни.
/// </summary>
public class UI_StrategyMap : MonoBehaviour
{
    /// <summary>
    /// Кнопка выбора первого уровня кампании.
    /// В будущем должна быть заменена массивом кнопок уровней.
    /// </summary>
    [Tooltip("Кнопка выбора Уровня 01. Временная реализация.")]
    [SerializeField] private Button level01Button;

    /// <summary>
    /// Кнопка запуска тактического сражения на выбранном уровне.
    /// Изначально неактивна, активируется после выбора уровня.
    /// Соответствует переходу: Стратегическая карта → Тактический уровень.
    /// </summary>
    [Tooltip("Кнопка 'Сражение' - запускает выбранный уровень.")]
    [SerializeField] private Button battleButton;

    /// <summary>
    /// Кнопка открытия интерфейса исследований (древа технологий).
    /// Предоставляет доступ к прокачке башен и способностей между уровнями.
    /// </summary>
    [Tooltip("Кнопка 'Исследования' - открывает древо технологий.")]
    [SerializeField] private Button researchButton;

    /// <summary>
    /// Внутренняя переменная для хранения имени выбранного уровня.
    /// Используется для передачи в SceneLoaderManager при запуске сражения.
    /// </summary>
    private string selectedLevel = "";

    /// <summary>
    /// Инициализирует интерфейс стратегической карты.
    /// Настраивает начальное состояние кнопок и назначает обработчики событий.
    /// </summary>
    private void Start()
    {
        // Изначально кнопка "Сражение" неактивна
        battleButton.interactable = false;

        level01Button.onClick.AddListener(() => OnLevelSelected("Level01"));
        battleButton.onClick.AddListener(OnBattleClicked);

        // TODO: Добавить обработчик для researchButton когда будет реализована система исследований
    }

    /// <summary>
    /// Обработчик выбора уровня на стратегической карте.
    /// Запоминает выбранный уровень и активирует кнопку запуска сражения.
    /// </summary>
    /// <param name="levelName">Идентификатор выбранного уровня.</param>
    private void OnLevelSelected(string levelName)
    {
        selectedLevel = levelName;
        Debug.Log($"Selected level: {selectedLevel}");

        // Активируем кнопку "Сражение" когда уровень выбран
        battleButton.interactable = true;

        // TODO: Нужно добавить визуальное выделение выбранного уровня
    }

    /// <summary>
    /// Обработчик нажатия кнопки "Сражение".
    /// Запускает загрузку тактического уровня, если уровень выбран.
    /// Выполняет переход на сцену тактического сражения.
    /// </summary>
    private void OnBattleClicked()
    {
        if (!string.IsNullOrEmpty(selectedLevel))
        {
            Debug.Log($"Starting battle on: {selectedLevel}");
            SceneLoaderManager.Instance.LoadTacticalLevel(selectedLevel);
        }
        else
        {
            Debug.LogWarning("No level selected!");
        }
    }
}