using System;
using UnityEngine;

/// <summary>
/// Синглтон-менеджер, отвечающий за управление и координацию глобальных состояний (GameState) игры.
/// Служит центральным узлом, который определяет, в каком режиме находится игра в данный момент, и уведомляет
/// все другие системы об изменениях этого состояния.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является "дирижером" игрового процесса. Изменение состояния игры через этот менеджер
/// является сигналом для всех других систем (UI, управления, звука, геймплея) переключить свой режим работы.</para>
/// <para><b>Паттерн Observer:</b> Предоставляет статическое событие <see cref="OnGameStateChanged"/>, позволяющее любым системам
/// подписаться на уведомления о смене состояния без прямой зависимости от менеджера.</para>
/// <para><b>Управление временем:</b> Отвечает за управление глобальной скоростью времени (Time.timeScale) при паузе.</para>
/// </remarks>
public class GameStateManager : MonoBehaviour
{
    /// <summary>
    /// Статическое свойство для глобального доступа к экземпляру менеджера.
    /// </summary>
    public static GameStateManager Instance { get; private set; }

    /// <summary>
    /// Текущее состояние игры. Изменение этого поля должно происходить только через метод <see cref="SetState"/>.
    /// </summary>
    private GameState currentState;

    /// <summary>
    /// Поле для сохранения состояния игры перед активацией состояния "Пауза"
    /// </summary>
    private GameState stateBeforePause;

    /// <summary>
    /// Публичное свойство только для чтения, предоставляющее доступ к текущему состоянию игры.
    /// </summary>
    public GameState CurrentState => currentState;

    /// <summary>
    /// Статическое событие, вызываемое при каждом изменении состояния игры.
    /// Параметр содержит новое состояние (<see cref="GameState"/>).
    /// </summary>
    public static event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Устанавливает начальное состояние игры "Главное меню".
    /// </summary>
    private void Start()
    {
        SetState(GameState.MainMenu);
    }

    /// <summary>
    /// Основной публичный метод для изменения состояния игры.
    /// Выполняет проверку на идентичность, обновление состояния, оповещение подписчиков и запуск специфичной логики.
    /// </summary>
    /// <param name="newState">Новое целевое состояние игры.</param>
    public void SetState(GameState newState)
    {
        // Защита от повторного установления идентичного состояния
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log($"[GameStateManager] Новое состояние игры: {newState}");

        // Оповещаем все подписанные системы об изменении состояния
        OnGameStateChanged?.Invoke(newState);

        // Выполняем логику, специфичную для нового состояния
        HandleStateSpecificLogic(newState);
    }

    /// <summary>
    /// Внутренний метод для обработки логики, специфичной для каждого состояния игры.
    /// В текущей реализации управляет глобальной скоростью времени (Time.timeScale).
    /// </summary>
    /// <param name="newState">Состояние, для которого применяется логика.</param>
    private void HandleStateSpecificLogic(GameState newState)
    {
        switch (newState)
        {
            case GameState.MainMenu:
                // Восстанавливаем нормальную скорость времени в главном меню
                Time.timeScale = 1f;
                break;

            case GameState.StrategyMap:
                // Восстанавливаем нормальную скорость времени на карте
                Time.timeScale = 1f;
                break;

            case GameState.TacticalLevel:
                // Восстанавливаем нормальную скорость времени на уровне
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                // Останавливаем время при паузе
                Time.timeScale = 0f;
                break;
        }
    }

    /// <summary>
    /// Публичный метод для переключения состояния паузы.
    /// Пауза работает поверх любого текущего состояния, кроме главного меню.
    /// При повторном вызове возвращает игру в состояние, актуальное до паузы.
    /// </summary>
    public void TogglePause()
    {
        // Пауза недопустима в главном меню
        if (currentState == GameState.MainMenu) return;

        if (currentState != GameState.Paused)
        {
            stateBeforePause = currentState; // Сохраняем состояние перед паузой
            SetState(GameState.Paused);
        }
        else
        {
            SetState(stateBeforePause); // Возвращаемся к сохраненному состоянию
        }
    }
}