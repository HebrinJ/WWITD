using UnityEngine;

/// <summary>
/// Компонент-маркер, обозначающий место на игровом поле, доступное для строительства башни.
/// Обрабатывает клики игрока, уведомляет систему строительства о выборе и обеспечивает визуальную обратную связь.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является "точкой взаимодействия" между игроком и системой строительства.
/// Не содержит логики строительства, только визуальное представление места, обработку ввода и состояние выбора.</para>
/// <para><b>Визуальное состояние:</b> Управляет сменой материалов для индикации выбора. При выборе меняет цвет на зеленый.</para>
/// </remarks>
public class TowerPlaceholder : MonoBehaviour
{
    [Header("Visual Feedback Settings")]
    [Tooltip("Рендерер объекта плейсхолдера для смены материалов.")]
    [SerializeField] private Renderer placeholderRenderer;

    [Tooltip("Материал для нормального состояния плейсхолдера.")]
    [SerializeField] private Material normalMaterial;

    [Tooltip("Материал для состояния выбора (зеленый цвет).")]
    [SerializeField] private Material selectedMaterial;

    /// <summary>
    /// Флаг, указывающий на текущее состояние выбора плейсхолдера.
    /// </summary>
    private bool isSelected = false;

    /// <summary>
    /// Инициализация компонента при старте. Настраивает визуальные компоненты.
    /// </summary>
    private void Start()
    {
        // Автоматически находим рендерер если не назначен в инспекторе
        if (placeholderRenderer == null)
        {
            placeholderRenderer = GetComponent<Renderer>();
        }

        // Устанавливаем начальное визуальное состояние
        SetNormalAppearance();
    }

    /// <summary>
    /// Обработчик события клика (тапа) по объекту плейсхолдера.
    /// Вызывается системой ввода Unity при нажатии на коллайдер объекта.
    /// </summary>
    private void OnMouseDown()
    {
        // При клике на этот объект сообщаем системе строительства, что он выбран
        SelectPlaceholder();
    }

    /// <summary>
    /// Метод выбора плейсхолдера. Вызывается при клике игрока.
    /// Устанавливает визуальное состояние "выбрано" и уведомляет систему.
    /// </summary>
    public void SelectPlaceholder()
    {
        if (isSelected) return;

        isSelected = true;
        SetSelectedAppearance();

        EventHub.OnTowerPlaceholderSelected?.Invoke(this);
        Debug.Log($"[TowerPlaceholder] Выбран плейсхолдер: {gameObject.name}");
    }

    /// <summary>
    /// Метод отмены выбора плейсхолдера.
    /// Вызывается при закрытии панели выбора или выборе другого плейсхолдера.
    /// </summary>
    public void DeselectPlaceholder()
    {
        if (!isSelected) return;

        isSelected = false;
        SetNormalAppearance();
        Debug.Log($"[TowerPlaceholder] Снят выбор с плейсхолдера: {gameObject.name}");
    }

    /// <summary>
    /// Устанавливает визуальное представление для состояния "выбрано".
    /// Применяет зеленый материал к рендереру плейсхолдера.
    /// </summary>
    private void SetSelectedAppearance()
    {
        if (placeholderRenderer != null && selectedMaterial != null)
        {
            placeholderRenderer.material = selectedMaterial;
        }
    }

    /// <summary>
    /// Устанавливает визуальное представление для нормального состояния.
    /// Возвращает стандартный материал плейсхолдера.
    /// </summary>
    private void SetNormalAppearance()
    {
        if (placeholderRenderer != null && normalMaterial != null)
        {
            placeholderRenderer.material = normalMaterial;
        }
    }

    // NOTE: Для будущего развития нужно добавить:
    // - Проверку доступности места для строительства (занято, заблокировано)
    // - Разные типы плейсхолдеров для разных типов башен
    // - Анимации перехода между состояниями
}