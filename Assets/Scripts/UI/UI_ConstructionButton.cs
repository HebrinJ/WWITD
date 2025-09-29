using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-компонент кнопки переключения режима строительства на тактическом уровне.
/// Отображает текущее состояние режима строительства и позволяет игроку включать/выключать его.
/// Взаимодействует с системой строительства через <see cref="EventHub"/>.
/// </summary>
public class UI_ConstructionButton : MonoBehaviour
{
    /// <summary>
    /// Ссылка на компонент Button, который обрабатывает нажатия.
    /// Должен быть назначен в инспекторе Unity.
    /// </summary>
    [Tooltip("Компонент Button для обработки нажатий.")]
    [SerializeField] private Button constructionButton;

    [Tooltip("Компонент Button для обработки нажатий.")]
    [SerializeField] private TextMeshProUGUI constructionModeDisplay;

    /// <summary>
    /// Инициализирует кнопку и подписывается на события при создании компонента.
    /// </summary>
    private void Start()
    {
        constructionButton.onClick.AddListener(ToggleConstructionMode);
        constructionModeDisplay.gameObject.SetActive(false);
    }

    /// <summary>
    /// Обработчик нажатия на кнопку строительства.
    /// Отправляет событие через EventHub о том, что игрок запросил переключение режима строительства.
    /// Фактическое переключение логики выполняется в <see cref="BuildManager"/>.
    /// </summary>
    private void ToggleConstructionMode()
    {
        EventHub.OnConstructionModeToggled?.Invoke();
    }

    private void OnEnable()
    {
        EventHub.OnConstructionModeChanged += ShowModeState;
    }

    private void OnDisable()
    {
        EventHub.OnConstructionModeChanged -= ShowModeState;
    }

    private void ShowModeState(bool state)
    {
        constructionModeDisplay.gameObject.SetActive(state);
    }
}