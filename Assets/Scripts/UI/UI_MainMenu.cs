using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-компонент главного меню игры. Точка входа в игровой процесс.
/// Обрабатывает навигацию между разделами меню и запуск игры.
/// Содержит кнопки: Начать игру, Настройки, Выход.
/// </summary>
public class UI_MainMenu : MonoBehaviour
{
    /// <summary>
    /// Кнопка перехода к стратегической карте кампании.
    /// Запускает основную игровую петлю.
    /// </summary>
    [Tooltip("Кнопка 'Начать игру' - переход на стратегическую карту.")]
    [SerializeField] private Button startGameButton;

    /// <summary>
    /// Кнопка открытия меню настроек игры.
    /// Позволяет настроить громкость звука, язык и другие параметры.
    /// </summary>
    [Tooltip("Кнопка 'Настройки' - открывает меню настроек.")]
    [SerializeField] private Button settingsButton;

    /// <summary>
    /// Кнопка выхода из игры.
    /// Завершает работу приложения.
    /// </summary>
    [Tooltip("Кнопка 'Выход' - завершает работу игры.")]
    [SerializeField] private Button exitButton;

    /// <summary>
    /// Инициализирует главное меню при старте.
    /// Назначает обработчики нажатий для всех кнопок меню.
    /// Гарантирует нормальную скорость течения времени (на случай возврата из паузы).
    /// </summary>
    private void Start()
    {
        startGameButton.onClick.AddListener(OnStartGameClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        // Убеждаемся, что время течет нормально
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Обработчик нажатия кнопки "Начать игру".
    /// Выполняет переход на сцену стратегической карты кампании.
    /// Соответствует первому шагу игровой петли: Главное меню → Стратегическая карта.
    /// </summary>
    private void OnStartGameClicked()
    {
        Debug.Log("Start Game button clicked");
        // Загружаем стратегическую карту
        SceneLoaderManager.Instance.LoadStrategyMap();
    }

    /// <summary>
    /// Обработчик нажатия кнопки "Настройки".
    /// Открывает меню настроек игры (громкость звука, выбор языка).
    /// </summary>
    private void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked");
        // TODO: Реализовать открытие настроек
    }

    /// <summary>
    /// Обработчик нажатия кнопки "Выход".
    /// Завершает работу приложения. В редакторе Unity останавливает воспроизведение.
    /// </summary>
    private void OnExitClicked()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}