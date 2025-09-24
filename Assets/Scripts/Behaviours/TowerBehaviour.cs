using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Основной компонент, управляющий поведением, характеристиками и атаками башни на тактическом уровне.
/// Отвечает за поиск целей, стрельбу, применение модификаторов от исследований и визуализацию радиуса атаки.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является "исполнителем" данных из <see cref="TowerDataSO"/>.
/// Управляет жизненным циклом атаки: поиск цели -> применение модификаторов -> создание снаряда.</para>
/// <para><b>Система модификаторов:</b> Поддерживает два типа бонусов от исследований - аддитивные (+X) и мультипликативные (+X%),
/// которые динамически изменяют характеристики башни.</para>
/// </remarks>
public class TowerBehaviour : MonoBehaviour
{
    [Header("Tower Parameters")]
    [SerializeField]
    [Tooltip("Точка спавна снарядов.")]
    private Transform projectileSpawnPoint;

    [SerializeField]
    [Tooltip("Данные башни (ScriptableObject), содержащие базовые характеристики.")]
    private TowerDataSO data;

    [Header("Target System")]
    [SerializeField]
    [Tooltip("Режим приоритета выбора целей.")]
    private TargetPriorityMode targetPriorityMode = TargetPriorityMode.Closest;

    /// <summary>
    /// Система управления целями башни. Отвечает за поиск и валидацию целей.
    /// </summary>
    private TowerTargetSystem targetSystem;

    /// <summary>
    /// Ссылка на корутину атаки для возможности ее остановки.
    /// </summary>
    private Coroutine attackCoroutine;

    /// <summary>
    /// Флаг активности башни. Если false, корутина атаки останавливается.
    /// </summary>
    private bool isActive = true;

    /// <summary>
    /// Словарь для хранения аддитивных бонусов (+X к характеристике).
    /// Ключ - название характеристики, значение - величина бонуса.
    /// </summary>
    private Dictionary<string, int> flatBonuses = new Dictionary<string, int>();

    /// <summary>
    /// Словарь для хранения мультипликативных бонусов (+X% к характеристике).
    /// Ключ - название характеристики, значение - множитель (1.0 = 100%, 1.25 = +25%).
    /// </summary>
    private Dictionary<string, float> multiplicativeBonuses = new Dictionary<string, float>();

    // Базовые характеристики (кэшируются из data для применения модификаторов)
    private int Damage;
    private float baseFireRate;
    private float baseFireDistance;
    private int baseHealth;
    private int baseArmor;

    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// Основной метод запуска поведения башни. Активирует корутину атаки.
    /// </summary>
    private void Start()
    {       
        if (data != null)
        {
            StartAttackRoutine();
        }
        else
        {
            Debug.LogError("TowerData is not assigned!", this);
        }
    }

    /// <summary>
    /// Основной метод инициализации башни
    /// </summary>
    private void Initialize()
    {
        // Кэшируем базовые значения из ScriptableObject
        Damage = data.damage;
        baseFireRate = data.fireRate;
        baseFireDistance = data.fireDistance;
        baseHealth = data.maxHealth;
        baseArmor = data.maxArmor;

        // Инициализация системы управления целями
        float currentRange = GetModifiedFireDistance();
        targetSystem = new TowerTargetSystem(this, targetPriorityMode, currentRange);
    }

    /// <summary>
    /// Метод для установки данных башни. Может вызываться динамически.
    /// </summary>
    /// <param name="data">Новые данные башни.</param>
    public void SetData(TowerDataSO data)
    {
        this.data = data;
        StartAttackRoutine();
    }

    // ================== СИСТЕМА МОДИФИКАТОРОВ ==================

    /// <summary>
    /// Возвращает модифицированное значение урона с учетом всех бонусов.
    /// </summary>
    /// <returns>Финальное значение урона.</returns>
    public int GetModifiedDamage()
    {
        int flatBonus = flatBonuses.ContainsKey("Damage") ? flatBonuses["Damage"] : 0;
        float multiplier = multiplicativeBonuses.ContainsKey("Damage") ? multiplicativeBonuses["Damage"] : 1f;

        return Mathf.RoundToInt((Damage + flatBonus) * multiplier);
    }

    /// <summary>
    /// Возвращает модифицированную скорость атаки с учетом всех бонусов.
    /// </summary>
    /// <returns>Финальное значение скорости атаки (выстрелов в секунду).</returns>
    public float GetModifiedFireRate()
    {
        float flatBonus = flatBonuses.ContainsKey("FireRate") ? flatBonuses["FireRate"] : 0f;
        float multiplier = multiplicativeBonuses.ContainsKey("FireRate") ? multiplicativeBonuses["FireRate"] : 1f;

        return (baseFireRate + flatBonus) * multiplier;
    }

    /// <summary>
    /// Возвращает модифицированную дальность атаки с учетом всех бонусов.
    /// </summary>
    /// <returns>Финальное значение дальности атаки.</returns>
    public float GetModifiedFireDistance()
    {
        float flatBonus = flatBonuses.ContainsKey("FireDistance") ? flatBonuses["FireDistance"] : 0f;
        float multiplier = multiplicativeBonuses.ContainsKey("FireDistance") ? multiplicativeBonuses["FireDistance"] : 1f;
        
        return (baseFireDistance + flatBonus) * multiplier;
    }

    /// <summary>
    /// Возвращает модифицированное здоровье с учетом всех бонусов.
    /// </summary>
    /// <returns>Финальное значение здоровья.</returns>
    public int GetModifiedHealth()
    {
        int flatBonus = flatBonuses.ContainsKey("Health") ? flatBonuses["Health"] : 0;
        float multiplier = multiplicativeBonuses.ContainsKey("Health") ? multiplicativeBonuses["Health"] : 1f;

        return Mathf.RoundToInt((baseHealth + flatBonus) * multiplier);
    }

    /// <summary>
    /// Возвращает модифицированную броню с учетом всех бонусов.
    /// </summary>
    /// <returns>Финальное значение брони.</returns>
    public int GetModifiedArmor()
    {
        int flatBonus = flatBonuses.ContainsKey("Armor") ? flatBonuses["Armor"] : 0;
        float multiplier = multiplicativeBonuses.ContainsKey("Armor") ? multiplicativeBonuses["Armor"] : 1f;

        return Mathf.RoundToInt((baseArmor + flatBonus) * multiplier);
    }

    // ================== МЕТОДЫ ДЛЯ ИССЛЕДОВАНИЙ ==================

    /// <summary>
    /// Применяет аддитивный бонус к характеристике.
    /// </summary>
    /// <param name="statName">Название характеристики ("Damage", "FireRate", etc.).</param>
    /// <param name="bonusValue">Величина бонуса.</param>
    public void ApplyFlatBonus(string statName, int bonusValue)
    {
        if (flatBonuses.ContainsKey(statName))
        {
            flatBonuses[statName] += bonusValue;
        }
        else
        {
            flatBonuses[statName] = bonusValue;
        }

        Debug.Log($"Applied flat bonus {bonusValue} to {statName}. Total flat: {flatBonuses[statName]}");
        LogCurrentStats();
    }

    /// <summary>
    /// Применяет мультипликативный бонус к характеристике.
    /// </summary>
    /// <param name="statName">Название характеристики.</param>
    /// <param name="bonusValue">Величина бонуса в долях (0.25 = +25%).</param>
    public void ApplyMultiplicativeBonus(string statName, float bonusValue)
    {
        if (multiplicativeBonuses.ContainsKey(statName))
        {
            multiplicativeBonuses[statName] += bonusValue;
        }
        else
        {
            multiplicativeBonuses[statName] = 1f + bonusValue;
        }

        Debug.Log($"Applied multiplicative bonus {bonusValue} to {statName}. Total multiplier: {multiplicativeBonuses[statName]}");
        LogCurrentStats();
    }

    /// <summary>
    /// Логирует текущие характеристики башни с учетом всех модификаторов.
    /// </summary>
    public void LogCurrentStats()
    {
        Debug.Log($"Tower {data.towerName} stats:\n" +
                 $"Damage: {GetModifiedDamage()} (base: {Damage} + flat: {GetFlatBonus("Damage")} * mult: {GetMultiplier("Damage")})\n" +
                 $"Fire Rate: {GetModifiedFireRate():F2} (base: {baseFireRate} + flat: {GetFlatBonus("FireRate")} * mult: {GetMultiplier("FireRate")})\n" +
                 $"Range: {GetModifiedFireDistance():F1} (base: {baseFireDistance} + flat: {GetFlatBonus("FireDistance")} * mult: {GetMultiplier("FireDistance")})");
    }

    /// <summary>
    /// Вспомогательный метод для получения аддитивного бонуса.
    /// </summary>
    private int GetFlatBonus(string statName)
    {
        return flatBonuses.ContainsKey(statName) ? flatBonuses[statName] : 0;
    }

    /// <summary>
    /// Вспомогательный метод для получения мультипликативного бонуса.
    /// </summary>
    private float GetMultiplier(string statName)
    {
        return multiplicativeBonuses.ContainsKey(statName) ? multiplicativeBonuses[statName] : 1f;
    }

    // ================== СИСТЕМА АТАКИ ==================

    /// <summary>
    /// Запускает корутину атаки. Останавливает предыдущую, если она активна.
    /// </summary>
    private void StartAttackRoutine()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    /// <summary>
    /// Корутина атаки с использованием системы управления целями.
    /// Сохраняет захват одной цели до ее уничтожения или выхода из радиуса.
    /// </summary>
    private IEnumerator AttackRoutine()
    {
        while (isActive)
        {
            // 1. Проверяем валидность текущей цели
            if (!targetSystem.IsCurrentTargetValid())
            {
                // 2. Если цель невалидна, ищем новую
                targetSystem.FindNewTarget();
            }

            // 3. Атакуем, если есть валидная цель
            if (targetSystem.HasTarget)
            {
                Attack();
            }

            // 4. Ожидание следующей атаки на основе модифицированной скорости
            yield return new WaitForSeconds(1f / GetModifiedFireRate());
        }
    }

    /// <summary>
    /// Метод атаки текущей цели из системы управления целями.
    /// Создает снаряд и настраивает его с учетом модифицированного урона.
    /// </summary>
    private void Attack()
    {
        if (!targetSystem.HasTarget) return;

        GameObject projectileObject = Instantiate(data.projectilePrefab,
                                                projectileSpawnPoint.position,
                                                projectileSpawnPoint.rotation);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        int finalDamage = GetModifiedDamage();

        Debug.Log($"{data.towerName} атакует {targetSystem.CurrentTarget.Data.enemyName} " +
                 $"на {finalDamage} урона!");

        if (projectile != null)
        {
            projectile.SetTarget(targetSystem.CurrentTarget.transform, finalDamage);
        }
        else
        {
            Debug.LogError("Projectile prefab doesn't have Projectile component!");
            Destroy(projectileObject);
        }
    }

    /// <summary>
    /// Обновляет радиус поиска целей при изменении характеристик башни.
    /// Вызывается после применения модификаторов от исследований.
    /// </summary>
    public void UpdateTargetingRange()
    {
        if (targetSystem != null && data != null)
        {
            targetSystem.UpdateTowerRange(GetModifiedFireDistance());
        }
    }

    /// <summary>
    /// Возвращает тип башни (для системы исследований и UI).
    /// </summary>
    /// <returns>Тип башни.</returns>
    public TowerType GetTowerType()
    {
        return data.towerType;
    }

    /// <summary>
    /// Визуализация радиуса атаки в редакторе Unity.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.fireDistance);
        }
    }

    /// <summary>
    /// Очистка при уничтожении объекта. Останавливает корутину атаки.
    /// </summary>
    private void OnDestroy()
    {
        isActive = false;
        if (this != null && attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
    }
}