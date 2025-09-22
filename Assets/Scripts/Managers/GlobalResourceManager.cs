using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Синглтон-менеджер, отвечающий за хранение и управление глобальными ресурсами игрока (Чертежами),
/// которые сохраняются между уровнями и используются для исследований на стратегической карте.
/// </summary>
/// <remarks>
/// <para><b>Глобальные vs. Локальные ресурсы:</b> Управляет только ресурсами, сохраняемыми между сессиями (в настоящее время только Чертежи).
/// Не управляет локальными ресурсами уровня (Деньги, Железо, Горючее), которые обрабатываются <see cref="LocalResourceManager"/>.</para>
/// <para><b>Паттерн Singleton:</b> Реализован как синглтон, сохраняемый между сценами (<see cref="DontDestroyOnLoad"/>),
/// что обеспечивает единую точку доступа к глобальным ресурсам из любой сцены (Главное меню, Карта, Уровень).</para>
/// <para><b>Роль в архитектуре:</b> Обеспечивает экономическую основу для системы исследований. Обрабатывает транзакции,
/// связанные с покупкой исследований на стратегической карте.</para>
/// </remarks>
public class GlobalResourceManager : MonoBehaviour
{
    /// <summary>
    /// Статическое свойство для глобального доступа к экземпляру менеджера.
    /// Реализует шаблон Singleton.
    /// </summary>
    public static GlobalResourceManager Instance { get; private set; }

    /// <summary>
    /// Словарь для хранения текущего количества каждого типа глобального ресурса.
    /// В текущей реализации предполагается хранение только ResourceType.Blueprints.
    /// </summary>
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    [SerializeField]
    [Tooltip("Стартовое количество Чертежей в новой игре.")]
    private int startBlueprints = 0;

    /// <summary>
    /// Метод инициализации синглтона.
    /// Уничтожает повторяющиеся экземпляры и помечает данный экземпляр как постоянный между сценами.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeResources();
    }

    private void OnEnable()
    {
        EventHub.OnGlobalResourceSpendRequested += HandleSpendRequest;
    }

    private void OnDisable()
    {
        EventHub.OnGlobalResourceSpendRequested -= HandleSpendRequest;
    }

    /// <summary>
    /// Инициализация начальных значений глобальных ресурсов.
    /// В текущей реализации инициализирует только чертежи.
    /// </summary>
    private void InitializeResources()
    {
        resources[ResourceType.Blueprints] = startBlueprints;
        EventHub.OnGlobalResourceChanged?.Invoke(ResourceType.Blueprints, resources[ResourceType.Blueprints]);
    }

    /// <summary>
    /// Обработчик события запроса на списание глобальных ресурсов.
    /// Вызывается системами, требующими проверки и списания ресурсов (например, ResearchManager при попытке исследования).
    /// </summary>
    /// <param name="cost">Структура, содержащая тип ресурса и количество для списания.</param>
    private void HandleSpendRequest(ResourceCost cost)
    {
        if (CanAfford(cost))
        {
            SpendResource(cost.resourceType, cost.amount);
        }
        else
        {
            Debug.LogWarning($"[GlobalResourceManager] Недостаточно глобального ресурса {cost.resourceType}!");
            // TODO: Вызвать событие OnGlobalSpendFailed для UI
        }
    }

    /// <summary>
    /// Обработчик события прямого добавления глобальных ресурсов.
    /// Вызывается для начисления ресурсов (например, при завершении уровня или выполнении задания).
    /// </summary>
    /// <param name="type">Тип добавляемого ресурса.</param>
    /// <param name="amount">Количество добавляемого ресурса.</param>
    private void HandleResourceAdded(ResourceType type, int amount)
    {
        AddResource(type, amount);
    }

    /// <summary>
    /// Публичный метод для проверки возможности списания указанной стоимости.
    /// </summary>
    /// <param name="cost">Структура, содержащая тип ресурса и требуемое количество.</param>
    /// <returns>True - если ресурсов достаточно, False - в противном случае.</returns>
    public bool CanAfford(ResourceCost cost)
    {
        return resources.ContainsKey(cost.resourceType) && resources[cost.resourceType] >= cost.amount;
    }

    /// <summary>
    /// Внутренний метод для списания глобальных ресурсов.
    /// Уменьшает счетчик ресурса и оповещает системы об изменении.
    /// </summary>
    /// <param name="type">Тип списываемого ресурса.</param>
    /// <param name="amount">Количество списываемого ресурса.</param>
    private void SpendResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] -= amount;
            EventHub.OnGlobalResourceChanged?.Invoke(type, resources[type]);
        }
    }

    /// <summary>
    /// Публичный метод для добавления глобальных ресурсов.
    /// Используется для начисления наград за уровни, задания и др.
    /// </summary>
    /// <param name="type">Тип добавляемого ресурса.</param>
    /// <param name="amount">Количество добавляемого ресурса.</param>
    public void AddResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] += amount;
            EventHub.OnGlobalResourceChanged?.Invoke(type, resources[type]);
            EventHub.OnGlobalResourceAdded?.Invoke(type, amount);
        }
        else
        {
            // Если ресурс ранее не был инициализирован, инициализируем его
            resources[type] = amount;
            EventHub.OnGlobalResourceChanged?.Invoke(type, resources[type]);
            EventHub.OnGlobalResourceAdded?.Invoke(type, amount);
        }
    }

    /// <summary>
    /// Публичный геттер для получения текущего количества конкретного глобального ресурса.
    /// </summary>
    /// <param name="type">Тип запрашиваемого ресурса.</param>
    /// <returns>Текущее количество ресурса. Если ресурс не найден, возвращает 0.</returns>
    public int GetResourceAmount(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }
}