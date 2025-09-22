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

    /// <summary>
    /// Компонент Image для отображения иконки кнопки.
    /// Меняет спрайт в зависимости от состояния режима строительства.
    /// </summary>
    [Tooltip("Компонент Image для отображения иконки кнопки.")]
    [SerializeField] private Image buttonIcon;

    /// <summary>
    /// Спрайт, отображаемый когда режим строительства активен.
    /// Обычно указывает на то, что повторное нажатие выключит режим.
    /// </summary>
    [Tooltip("Иконка, когда режим строительства ВКЛЮЧЕН.")]
    [SerializeField] private Sprite constructionOnIcon;

    /// <summary>
    /// Спрайт, отображаемый когда режим строительства не активен.
    /// Обычно указывает на то, что нажатие включит режим строительства.
    /// </summary>
    [Tooltip("Иконка, когда режим строительства ВЫКЛЮЧЕН.")]
    [SerializeField] private Sprite constructionOffIcon;

    /// <summary>
    /// Инициализирует кнопку и подписывается на события при создании компонента.
    /// Назначает обработчик нажатия кнопки и подписывается на изменение состояния режима строительства.
    /// </summary>
    private void Start()
    {
        constructionButton.onClick.AddListener(ToggleConstructionMode);
        EventHub.OnConstructionModeChanged += UpdateButtonIcon;
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

    /// <summary>
    /// Обновляет визуальное состояние кнопки на основе текущего режима строительства.
    /// Вызывается при получении события OnConstructionModeChanged из EventHub.
    /// </summary>
    /// <param name="isConstructionModeActive">True если режим строительства активен, иначе false.</param>
    private void UpdateButtonIcon(bool isConstructionModeActive)
    {
        buttonIcon.sprite = isConstructionModeActive ? constructionOnIcon : constructionOffIcon;
    }

    private void OnDestroy()
    {
        EventHub.OnConstructionModeChanged -= UpdateButtonIcon;
    }
}