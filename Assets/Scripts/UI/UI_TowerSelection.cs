using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-компонент панели выбора башни для строительства на тактическом уровне.
/// Отображается при выборе TowerPlaceholder и позволяет игроку выбрать тип башни для постройки.
/// Взаимодействует с системой строительства через <see cref="EventHub"/>.
/// </summary>
public class UI_TowerSelection : MonoBehaviour
{
    /// <summary>
    /// Родительский GameObject панели выбора башни.
    /// Содержит все UI-элементы панели и управляет их видимостью.
    /// </summary>
    [Tooltip("Родительский GameObject панели выбора башни.")]
    [SerializeField] private GameObject selectionPanel;

    /// <summary>
    /// Кнопка выбора пулемётного гнезда (Riflemen) для постройки.
    /// Временная реализация - должна быть расширена до системы динамических кнопок.
    /// </summary>
    [Tooltip("Кнопка выбора пулемётного гнезда. Временная реализация.")]
    [SerializeField] private Button machineGunButton;

    /// <summary>
    /// Кнопка закрытия панели выбора без строительства.
    /// Позволяет игроку отменить выбор места для строительства.
    /// </summary>
    [Tooltip("Кнопка закрытия панели выбора башни.")]
    [SerializeField] private Button closeButton;

    /// <summary>
    /// Ссылка на основную камеру сцены.
    /// Используется для преобразования мировых координат в экранные для позиционирования панели.
    /// </summary>
    [Tooltip("Основная камера сцены для позиционирования панели.")]
    [SerializeField] private Camera mainCamera;

    /// <summary>
    /// Текущий выбранный TowerPlaceholder, для которого отображается панель выбора.
    /// Хранится для передачи контекста при выборе типа башни.
    /// </summary>
    private TowerPlaceholder currentPlaceholder;

    /// <summary>
    /// Подписывается на событие выбора места для строительства при активации объекта.
    /// </summary>
    private void OnEnable()
    {
        EventHub.OnTowerPlaceholderSelected += ShowSelectionPanel;
    }

    /// <summary>
    /// Отписывается от событий при деактивации объекта для предотвращения утечек памяти.
    /// </summary>
    private void OnDisable()
    {
        EventHub.OnTowerPlaceholderSelected -= ShowSelectionPanel;
    }

    /// <summary>
    /// Инициализирует панель выбора башни при старте.
    /// Настраивает начальное состояние и назначает обработчики кнопок.
    /// </summary>
    private void Start()
    {
        selectionPanel.SetActive(false);

        machineGunButton.onClick.AddListener(() => SelectTower(TowerType.Riflemen));
        closeButton.onClick.AddListener(HideSelectionPanel);
    }

    /// <summary>
    /// Отображает панель выбора башни при выборе места для строительства.
    /// Позиционирует панель рядом с выбранным TowerPlaceholder в экранных координатах.
    /// Требует изменений после утверждения позиционирования UI
    /// </summary>
    /// <param name="placeholder">Выбранное место для строительства башни.</param>
    private void ShowSelectionPanel(TowerPlaceholder placeholder)
    {
        Debug.Log("Show Panel");
        currentPlaceholder = placeholder;
        selectionPanel.SetActive(true);

        // Позиционируем панель рядом с выбранным местом для строительства
        if (mainCamera != null)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(placeholder.transform.position);
            selectionPanel.transform.position = screenPosition + new Vector3(0, 100, 0);
        }
    }

    /// <summary>
    /// Скрывает панель выбора башни и сбрасывает текущий выбор.
    /// Вызывается при закрытии панели без выбора башни.
    /// </summary>
    private void HideSelectionPanel()
    {
        selectionPanel.SetActive(false);
        currentPlaceholder = null;
    }

    /// <summary>
    /// Обработчик выбора конкретного типа башни для строительства.
    /// Отправляет событие с выбранным типом башни через EventHub.
    /// Скрывает панель после выбора.
    /// </summary>
    /// <param name="towerType">Тип башни, выбранный для строительства.</param>
    private void SelectTower(TowerType towerType)
    {
        if (currentPlaceholder != null)
        {
            EventHub.OnTowerSelectedFromUI?.Invoke(towerType);
            HideSelectionPanel();
        }
    }
}