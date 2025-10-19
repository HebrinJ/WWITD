using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// UI-компонент стратегической карты кампании.
/// Предоставляет интерфейс для выбора уровней, запуска тактических сражений и доступа к исследованиям.
/// Отображает прогресс игрока и доступные для прохождения уровни.
/// </summary>
/// <remarks>
/// <para><b>Архитектура:</b> Использует предварительно размещенные кнопки уровней на сцене.
/// Подписывается на события EventHub для реактивного обновления UI.</para>
/// </remarks>
public class UI_StrategyMap : MonoBehaviour
{
    [Header("Level Buttons - Назначаются в инспекторе")]
    /// <summary>
    /// Массив кнопок уровней, предварительно размещенных на карте.
    /// Должны быть назначены в инспекторе Unity.
    /// </summary>
    [Tooltip("Кнопки уровней, размещенные на стратегической карте. Назначаются вручную.")]
    [SerializeField] private UI_LevelButton[] levelButtons;

    [Header("UI References")]
    /// <summary>
    /// Кнопка запуска тактического сражения на выбранном уровне.
    /// Изначально неактивна, активируется после выбора доступного уровня.
    /// </summary>
    [Tooltip("Кнопка 'Сражение' - запускает выбранный уровень.")]
    [SerializeField] private Button battleButton;

    /// <summary>
    /// Текст кнопки сражения для динамического обновления.
    /// </summary>
    [Tooltip("Текст кнопки 'Сражение'.")]
    [SerializeField] private TextMeshProUGUI battleButtonText;

    /// <summary>
    /// Словарь для быстрого доступа к кнопкам уровней по их идентификатору.
    /// </summary>
    private Dictionary<string, UI_LevelButton> levelButtonMap = new Dictionary<string, UI_LevelButton>();

    /// <summary>
    /// Текущий выбранный уровень.
    /// </summary>
    private LevelDataSO selectedLevel;

    private void Start()
    {
        InitializeStrategyMap();
    }

    private void OnEnable()
    {
        // Подписываемся на события из EventHub
        EventHub.OnLevelStateChanged += HandleLevelStateChanged;
        EventHub.OnSaveDataLoaded += HandleSaveDataLoaded;
        EventHub.OnLevelSelected += HandleLevelSelected;
    }

    private void OnDisable()
    {
        // Отписываемся от событий EventHub
        EventHub.OnLevelStateChanged -= HandleLevelStateChanged;
        EventHub.OnSaveDataLoaded -= HandleSaveDataLoaded;
        EventHub.OnLevelSelected -= HandleLevelSelected;
    }

    /// <summary>
    /// Инициализирует интерфейс стратегической карты.
    /// Настраивает кнопки уровней и их начальное состояние.
    /// </summary>
    private void InitializeStrategyMap()
    {
        levelButtonMap.Clear();

        // Проверяем наличие кнопок уровней
        if (levelButtons == null || levelButtons.Length == 0)
        {
            Debug.LogError("[UI_StrategyMap] Не назначены кнопки уровней в инспекторе!");
            return;
        }

        // Проверяем доступность менеджера
        if (StrategyMapManager.Instance == null)
        {
            Debug.LogError("[UI_StrategyMap] StrategyMapManager не найден!");
            return;
        }

        // Инициализируем кнопки уровней
        foreach (var levelButton in levelButtons)
        {
            if (levelButton != null)
            {
                // Получаем данные уровня для этой кнопки
                LevelDataSO levelData = StrategyMapManager.Instance.GetLevelData(levelButton.LevelId);
                if (levelData != null)
                {
                    // Инициализируем кнопку
                    levelButton.Initialize(levelData, OnLevelSelected);
                    levelButtonMap[levelData.levelId] = levelButton;

                    // Обновляем визуальное состояние
                    LevelState initialState = levelData.GetLevelState(SaveManager.Instance);
                    levelButton.UpdateVisualState(initialState);
                }
                else
                {
                    Debug.LogWarning($"[UI_StrategyMap] Не найдены данные для уровня: {levelButton.LevelId}");
                }
            }
        }

        // Изначально кнопка "Сражение" неактивна
        battleButton.interactable = false;
        battleButton.onClick.AddListener(OnBattleClicked);

        Debug.Log($"[UI_StrategyMap] Стратегическая карта инициализирована. Кнопок уровней: {levelButtonMap.Count}");
    }

    /// <summary>
    /// Обработчик загрузки данных сохранения.
    /// Обновляет состояние всех кнопок уровней после загрузки прогресса.
    /// </summary>
    /// <param name="playerData">Загруженные данные игрока.</param>
    private void HandleSaveDataLoaded(PlayerSaveData playerData)
    {
        UpdateAllLevelButtons();
    }

    /// <summary>
    /// Обработчик выбора уровня через EventHub.
    /// Обновляет UI при выборе уровня из других систем.
    /// </summary>
    /// <param name="levelData">Выбранный уровень.</param>
    private void HandleLevelSelected(LevelDataSO levelData)
    {
        // Обновляем выделение кнопок
        foreach (var button in levelButtons)
        {
            if (button != null)
            {
                button.SetSelected(button.LevelData == levelData);
            }
        }

        // Активируем кнопку "Сражение"
        battleButton.interactable = true;
        battleButtonText.text = $"В БОЙ: {levelData.displayName}";

        selectedLevel = levelData;
    }

    /// <summary>
    /// Обновляет визуальное состояние всех кнопок уровней.
    /// </summary>
    private void UpdateAllLevelButtons()
    {
        if (StrategyMapManager.Instance == null || SaveManager.Instance == null)
            return;

        foreach (var levelButton in levelButtons)
        {
            if (levelButton != null && levelButton.LevelData != null)
            {
                LevelState state = levelButton.LevelData.GetLevelState(SaveManager.Instance);
                levelButton.UpdateVisualState(state);
            }
        }

        Debug.Log("[UI_StrategyMap] Состояние всех кнопок уровней обновлено");
    }

    /// <summary>
    /// Обработчик выбора уровня на стратегической карте.
    /// Вызывается при нажатии на кнопку уровня.
    /// </summary>
    /// <param name="levelData">Выбранный уровень.</param>
    private void OnLevelSelected(LevelDataSO levelData)
    {
        // Выбираем уровень через менеджер (он оповестит через EventHub)
        StrategyMapManager.Instance.SelectLevel(levelData);
    }

    /// <summary>
    /// Обработчик нажатия кнопки "Сражение".
    /// Запускает выбранный уровень через StrategyMapManager.
    /// </summary>
    private void OnBattleClicked()
    {
        if (selectedLevel != null)
        {
            StrategyMapManager.Instance.LaunchSelectedLevel();
        }
        else
        {
            Debug.LogWarning("[UI_StrategyMap] Не выбран уровень для запуска!");
        }
    }

    /// <summary>
    /// Обработчик изменения состояния уровня через EventHub.
    /// Обновляет визуальное представление соответствующей кнопки уровня.
    /// </summary>
    /// <param name="levelData">Данные уровня.</param>
    /// <param name="newState">Новое состояние уровня.</param>
    private void HandleLevelStateChanged(LevelDataSO levelData, LevelState newState)
    {
        if (levelButtonMap.TryGetValue(levelData.levelId, out UI_LevelButton button))
        {
            button.UpdateVisualState(newState);
        }
    }

    /// <summary>
    /// Вспомогательный метод для поиска кнопки уровня по идентификатору.
    /// </summary>
    /// <param name="levelId">Идентификатор уровня.</param>
    /// <returns>Кнопка уровня или null если не найдена.</returns>
    public UI_LevelButton GetLevelButton(string levelId)
    {
        levelButtonMap.TryGetValue(levelId, out UI_LevelButton button);
        return button;
    }
}