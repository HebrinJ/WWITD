using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-компонент кнопки открытия/закрытия интерфейса исследований (древа технологий).
/// Обеспечивает доступ к системе прокачки башен и способностей со стратегической карты.
/// Контролирует доступность функции исследований в зависимости от состояния игры.
/// </summary>
public class UI_ResearchButton : MonoBehaviour
{
    /// <summary>
    /// Ссылка на компонент Button для обработки нажатий.
    /// Должен быть назначен в инспекторе Unity.
    /// </summary>
    [Tooltip("Кнопка открытия панели исследований.")]
    [SerializeField] private Button researchButton;

    /// <summary>
    /// Инициализирует кнопку исследований при старте.
    /// Назначает обработчик нажатия на кнопку.
    /// </summary>
    private void Start()
    {
        researchButton.onClick.AddListener(OnResearchButtonClick);
    }

    /// <summary>
    /// Обработчик нажатия на кнопку исследований.
    /// Проверяет доступность исследований в текущем состоянии игры.
    /// Открывает или закрывает панель исследований через UIManager.
    /// </summary>
    private void OnResearchButtonClick()
    {
        // Исследования доступны только на стратегической карте, но не в главном меню
        if (GameStateManager.Instance.CurrentState == GameState.MainMenu)
        {
            Debug.LogWarning("Research not available in Main Menu");
            return;
        }

        // Переключаем видимость панели исследований
        UI_Manager.Instance.ToggleResearchPanel();
    }
}