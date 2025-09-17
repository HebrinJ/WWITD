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
                break;

            case GameState.StrategyMap:
                Time.timeScale = 1f;
                break;

            case GameState.TacticalLevel:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;
        }
    }

    public void TogglePause()
    {
        // ѕауза работает поверх любого состо€ни€ кроме главного меню
        if (currentState == GameState.MainMenu) return;

        SetState(currentState == GameState.Paused ?
                GetPreviousState() : // ¬озвращаемс€ к предыдущему состо€нию
                GameState.Paused);
    }

    private GameState GetPreviousState()
    {
        // «десь можно хранить историю состо€ний
        // ѕока просто возвращаемс€ к стратегической карте или тактическому уровню
        return GameState.StrategyMap;
    }
}
