using TMPro;
using UnityEngine;

/// <summary>
/// UI-компонент для отображения количества конкретного ресурса в интерфейсе игры.
/// Может отображать как локальные (внутриуровневые), так и глобальные (межуровневые) ресурсы.
/// Автоматически обновляет отображение при изменении ресурсов через систему событий EventHub.
/// </summary>
public class UI_ResourceDisplay : MonoBehaviour
{
    /// <summary>
    /// Тип ресурса, который отображает этот UI-элемент.
    /// Определяет, какое значение будет показываться (деньги, железо, горючее, чертежи).
    /// </summary>
    [Tooltip("Тип отображаемого ресурса (Money, Iron, Fuel, Blueprints).")]
    [SerializeField] private ResourceType resourceType;

    /// <summary>
    /// TextMeshPro компонент для отображения числового значения ресурса.
    /// Должен быть назначен в инспекторе Unity.
    /// </summary>
    [Tooltip("TextMeshPro компонент для отображения количества ресурса.")]
    [SerializeField] private TextMeshProUGUI amountText;

    /// <summary>
    /// Определяет, является ли ресурс глобальным (true) или локальным (false).
    /// Глобальные ресурсы: чертежи (сохраняются между уровнями).
    /// Локальные ресурсы: деньги, железо, горючее (действительны только на текущем уровне).
    /// </summary>
    [Tooltip("True - глобальный ресурс (чертежи), False - локальный ресурс (деньги, железо, горючее).")]
    [SerializeField] private bool isGlobalResource;

    /// <summary>
    /// Подписывается на соответствующие события при активации объекта.
    /// В зависимости от типа ресурса подписывается на глобальные или локальные события изменений.
    /// Для глобальных ресурсов сразу обновляет отображение текущим значением.
    /// </summary>
    private void OnEnable()
    {
        // Подписываемся на правильное событие в зависимости от типа ресурса
        if (isGlobalResource)
        {
            EventHub.OnGlobalResourceChanged += HandleResourceChanged;
            UpdateGlobalResourceDisplay();
        }
        else
        {
            EventHub.OnLocalResourceChanged += HandleResourceChanged;
        }
    }

    private void OnDisable()
    {
        if (isGlobalResource)
        {
            EventHub.OnGlobalResourceChanged -= HandleResourceChanged;
        }
        else
        {
            EventHub.OnLocalResourceChanged -= HandleResourceChanged;
        }
    }

    /// <summary>
    /// Обработчик изменения ресурса. Вызывается при получении соответствующего события из EventHub.
    /// Обновляет текстовое отображение, если измененный ресурс соответствует типу этого дисплея.
    /// </summary>
    /// <param name="type">Тип ресурса, который изменился.</param>
    /// <param name="amount">Новое количество ресурса.</param>
    private void HandleResourceChanged(ResourceType type, int amount)
    {
        if (type == resourceType)
        {
            amountText.text = amount.ToString();
        }
    }

    /// <summary>
    /// Обновляет отображение глобального ресурса, запрашивая актуальное значение у GlobalResourceManager.
    /// Используется при инициализации для установки начального значения.
    /// </summary>
    private void UpdateGlobalResourceDisplay()
    {
        if (isGlobalResource && GlobalResourceManager.Instance != null)
        {
            int amount = GlobalResourceManager.Instance.GetResourceAmount(resourceType);
            amountText.text = amount.ToString();
        }
    }

    /// <summary>
    /// Публичный метод для принудительного обновления отображения ресурса.
    /// Может использоваться для прямого обновления без использования системы событий.
    /// </summary>
    /// <param name="type">Тип ресурса для обновления.</param>
    /// <param name="amount">Новое количество ресурса.</param>
    public void UpdateResourceDisplay(ResourceType type, int amount)
    {
        if (type == resourceType)
        {
            amountText.text = amount.ToString();
        }
    }
}