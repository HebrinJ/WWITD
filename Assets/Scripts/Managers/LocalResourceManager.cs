using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Менеджер, отвечающий за хранение, отслеживание и операции с локальными ресурсами в рамках одного тактического уровня.
/// Обрабатывает начисление и списание ресурсов, проверяет возможность совершения транзакций и уведомляет другие системы об изменениях.
/// </summary>
/// <remarks>
/// <para><b>Локальные vs. Глобальные ресурсы:</b> Управляет только ресурсами, используемыми на уровне (Деньги, Железо, Горючее).
/// Не управляет глобальными чертежами (Blueprints), которые обрабатываются отдельной системой.</para>
/// <para><b>Роль в архитектуре:</b> Является основным подписчиком на события, связанные с изменением ресурсов на уровне.</para>
/// </remarks>
public class LocalResourceManager : MonoBehaviour
{
    /// <summary>
    /// Словарь для хранения текущего количества каждого типа локального ресурса.
    /// </summary>
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    [SerializeField]
    [Tooltip("Стартовое количество денег на уровне.")]
    private int startMoney = 100;

    [SerializeField]
    [Tooltip("Стартовое количество железа на уровне.")]
    private int startIron = 50;

    [SerializeField]
    [Tooltip("Стартовое количество горючего на уровне.")]
    private int startFuel = 25;

    /// <summary>
    /// Инициализирует стартовые значения ресурсов при запуске уровня и оповещает UI об их наличии.
    /// </summary>
    private void Start()
    {
        InitializeResources();
    }

    private void OnEnable()
    {
        EventHub.OnLocalResourceSpendRequested += HandleSpendRequest;
        EventHub.OnLocalResourceAdded += HandleResourceAdded;
        EventHub.OnEnemyDiedForMoney += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EventHub.OnLocalResourceSpendRequested -= HandleSpendRequest;
        EventHub.OnLocalResourceAdded -= HandleResourceAdded;
        EventHub.OnEnemyDiedForMoney -= HandleEnemyDied;
    }

    /// <summary>
    /// Метод инициализации ресурсов в начале уровня. Заполняет словарь стартовыми значениями
    /// и немедленно оповещает все заинтересованные системы (в первую очередь UI) о текущем количестве каждого ресурса.
    /// </summary>
    private void InitializeResources()
    {
        resources[ResourceType.Money] = startMoney;
        resources[ResourceType.Iron] = startIron;
        resources[ResourceType.Fuel] = startFuel;

        // Оповещаем UI о стартовых значениях каждого ресурса
        foreach (var resource in resources)
        {
            EventHub.OnLocalResourceChanged?.Invoke(resource.Key, resource.Value);
        }
    }

    /// <summary>
    /// Обработчик события запроса на списание ресурсов. Вызывается другими системами (например, BuildManager) при попытке покупки.
    /// Проверяет, достаточно ли ресурсов, и если да - списывает их.
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
            Debug.LogWarning($"[LocalResourceManager] Недостаточно {cost.resourceType} для списания {cost.amount}!");
            // TODO: Возможно, стоит вызвать отдельное событие о неудачной транзакции (e.g., OnSpendFailed)
        }
    }

    /// <summary>
    /// Обработчик события прямого добавления ресурсов. Вызывается для начисления ресурсов извне (например, при добыче из карьера).
    /// </summary>
    /// <param name="type">Тип добавляемого ресурса.</param>
    /// <param name="amount">Количество добавляемого ресурса.</param>
    private void HandleResourceAdded(ResourceType type, int amount)
    {
        AddResource(type, amount);
    }

    /// <summary>
    /// Обработчик события смерти врага. Начисляет денежную награду за убийство, если данные врага содержат информацию о награде.
    /// </summary>
    /// <param name="enemy">Поведение (Behaviour) убитого врага, содержащее ссылку на его данные (Data).</param>
    private void HandleEnemyDied(EnemyBehaviour enemy)
    {
        if (enemy != null && enemy.Data != null)
        {
            AddResource(ResourceType.Money, enemy.Data.rewardMoney);
        }
    }

    /// <summary>
    /// Публичный метод для проверки, достаточно ли ресурсов для покрытия указанной стоимости.
    /// </summary>
    /// <param name="cost">Структура, содержащая тип ресурса и требуемое количество.</param>
    /// <returns>True - если ресурсов достаточно, False - в противном случае.</returns>
    public bool CanAfford(ResourceCost cost)
    {
        return resources.ContainsKey(cost.resourceType) && resources[cost.resourceType] >= cost.amount;
    }

    /// <summary>
    /// Удобная перегрузка метода проверки для прямого указания типа и количества.
    /// </summary>
    /// <param name="type">Тип ресурса для проверки.</param>
    /// <param name="amount">Требуемое количество ресурса.</param>
    /// <returns>True - если ресурсов достаточно, False - в противном случае.</returns>
    public bool CanAfford(ResourceType type, int amount)
    {
        return CanAfford(new ResourceCost(type, amount));
    }

    /// <summary>
    /// Внутренний метод для списания ресурсов. Уменьшает счетчик ресурса и оповещает системы об изменении.
    /// Вызывается после успешной проверки CanAfford.
    /// </summary>
    /// <param name="type">Тип списываемого ресурса.</param>
    /// <param name="amount">Количество списываемого ресурса.</param>
    private void SpendResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] -= amount;
            EventHub.OnLocalResourceChanged?.Invoke(type, resources[type]);
        }
    }

    /// <summary>
    /// Внутренний метод для добавления ресурсов. Увеличивает счетчик ресурса и оповещает системы об изменении.
    /// </summary>
    /// <param name="type">Тип добавляемого ресурса.</param>
    /// <param name="amount">Количество добавляемого ресурса.</param>
    private void AddResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] += amount;
            EventHub.OnLocalResourceChanged?.Invoke(type, resources[type]);
            EventHub.OnLocalResourceAdded?.Invoke(type, amount);
        }
    }

    /// <summary>
    /// Публичный геттер для получения текущего количества конкретного ресурса.
    /// </summary>
    /// <param name="type">Тип запрашиваемого ресурса.</param>
    /// <returns>Текущее количество ресурса. Если ресурс не найден, возвращает 0.</returns>
    public int GetResourceAmount(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }
}