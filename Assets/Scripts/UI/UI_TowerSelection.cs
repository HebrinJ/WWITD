using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    /// Текущий выбранный TowerPlaceholder, для которого отображается панель выбора.
    /// Хранится для передачи контекста при выборе типа башни.
    /// </summary>
    private TowerPlaceholder currentPlaceholder;

    /// <summary>
    /// Флаг, указывающий что панель в данный момент активна.
    /// Используется для обработки кликов вне панели.
    /// </summary>
    private bool isPanelActive = false;

    private Coroutine ignoreClickCoroutine;
    private bool ignoreNextClick = false;

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
        isPanelActive = false;

        machineGunButton.onClick.AddListener(() => SelectTower(TowerType.Riflemen));
        closeButton.onClick.AddListener(HideSelectionPanel);
    }

    /// <summary>
    /// Обрабатывает ввод каждый кадр. Проверяет клики вне панели когда она активна.
    /// </summary>
    private void Update()
    {
        if (!isPanelActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (ignoreNextClick)
            {
                ignoreNextClick = false;
                return;
            }

            if (!IsPointerOverSelectionPanel())
            {
                HideSelectionPanel();
            }
        }
    }

    private void ShowSelectionPanel(TowerPlaceholder placeholder)
    {
        if (currentPlaceholder != null && currentPlaceholder != placeholder)
        {
            currentPlaceholder.DeselectPlaceholder();
        }

        currentPlaceholder = placeholder;
        isPanelActive = true;
        selectionPanel.SetActive(true);

        // Запускаем корутину для игнорирования кликов в этом кадре
        if (ignoreClickCoroutine != null) StopCoroutine(ignoreClickCoroutine);
        ignoreClickCoroutine = StartCoroutine(IgnoreClicksThisFrame());

        Debug.Log($"[UI_TowerSelection] Отображена панель выбора");
    }

    /// <summary>
    /// Игнорирует клики в течение текущего кадра после открытия панели.
    /// </summary>
    private IEnumerator IgnoreClicksThisFrame()
    {
        ignoreNextClick = true;
        yield return new WaitForEndOfFrame();
        ignoreNextClick = false;
    }

    /// <summary>
    /// Проверяет, находится ли указатель мыши над элементами панели выбора башни.
    /// </summary>
    /// <returns>true если указатель над панелью выбора, иначе false.</returns>
    private bool IsPointerOverSelectionPanel()
    {
        if (EventSystem.current == null) return false;

        // Создаем данные события указателя
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // Получаем список всех UI-объектов под указателем
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // Проверяем, есть ли среди результатов элементы нашей панели выбора
        foreach (var result in results)
        {
            // Если кликнули по любому элементу, который является частью панели выбора
            if (result.gameObject.transform.IsChildOf(selectionPanel.transform))
            {
                return true;
            }
        }

        return false;
    }

    ///// <summary>
    ///// Отображает панель выбора башни и активирует фоновую панель.
    ///// </summary>
    //private void ShowSelectionPanel(TowerPlaceholder placeholder)
    //{
    //    if (currentPlaceholder != null && currentPlaceholder != placeholder)
    //    {
    //        currentPlaceholder.DeselectPlaceholder();
    //    }

    //    currentPlaceholder = placeholder;
    //    isPanelActive = true;

    //    Debug.Log($"[UI_TowerSelection] Отображена панель выбора");
    //}

    /// <summary>
    /// Скрывает панель выбора и фоновую панель.
    /// </summary>
    private void HideSelectionPanel()
    {
        if (!isPanelActive) return;

        isPanelActive = false;
        selectionPanel.SetActive(false);

        // Снимаем выделение с плейсхолдера
        if (currentPlaceholder != null)
        {
            currentPlaceholder.DeselectPlaceholder();
            currentPlaceholder = null;
        }
    }

    /// <summary>
    /// Обработчик выбора конкретного типа башни для строительства.
    /// Отправляет событие с выбранным типом башни через EventHub и скрывает панель.
    /// </summary>
    /// <param name="towerType">Тип башни, выбранный для строительства.</param>
    private void SelectTower(TowerType towerType)
    {
        if (currentPlaceholder != null)
        {
            EventHub.OnTowerSelectedFromUI?.Invoke(towerType);
            Debug.Log($"[UI_TowerSelection] Выбрана башня типа: {towerType}");
        }

        HideSelectionPanel();
    }
}