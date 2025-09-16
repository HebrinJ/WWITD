using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    private GameState currentState;
    public GameState CurrentState => currentState;

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

    private void Start()
    {
        SetState(GameState.MainMenu);
    }

    public void SetState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log($"Game State: {newState}");
        OnGameStateChanged?.Invoke(newState);

        HandleStateSpecificLogic(newState);
    }

    private void HandleStateSpecificLogic(GameState newState)
    {
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                // Загружаем сцену главного меню
                break;

            case GameState.StrategyMap:
                // Загружаем стратегическую карту
                // Показываем пройденные уровни, доступные миссии
                break;

            case GameState.TacticalLevel:
                Time.timeScale = 1f;
                // Инициализируем уровень: спавним штаб, врагов, ресурсы. Первая волна спавнится после нажатия кнопки старт
                break;

            case GameState.Paused:
                Time.timeScale = 0f; // Главное - останавливаем время!
                break;
        }
    }

    public void TogglePause()
    {
        // Пауза работает поверх любого состояния кроме главного меню
        if (currentState == GameState.MainMenu) return;

        SetState(currentState == GameState.Paused ?
                GetPreviousState() : // Возвращаемся к предыдущему состоянию
                GameState.Paused);
    }

    private GameState GetPreviousState()
    {
        // Здесь можно хранить историю состояний
        // Пока просто возвращаемся к стратегической карте или тактическому уровню
        return GameState.StrategyMap;
    }
}
