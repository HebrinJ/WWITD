using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Система поиска и управления целями для башни.
/// Отвечает за выбор цели по различным алгоритмам и отслеживание текущей цели.
/// Обеспечивает сохранение захвата одной цели до ее уничтожения или выхода из радиуса.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является компонентом системы управления целями башни.
/// Инкапсулирует логику поиска и валидации целей, освобождая <see cref="TowerBehaviour"/> от этих обязанностей.</para>
/// <para><b>Принцип работы:</b> Система проверяет валидность текущей цели каждый цикл атаки.
/// Если цель становится невалидной (уничтожена или вышла из радиуса), ищет новую цель согласно выбранному алгоритму.</para>
/// </remarks>
public class TowerTargetSystem
{
    private TowerBehaviour tower;
    private TargetPriorityMode currentPriorityMode;
    private EnemyBehaviour currentTarget;
    private float acquisitionRange;

    /// <summary>
    /// Текущая цель башни. Может быть null если цель не найдена.
    /// </summary>
    public EnemyBehaviour CurrentTarget => currentTarget;

    /// <summary>
    /// Флаг наличия активной цели.
    /// </summary>
    public bool HasTarget => currentTarget != null;

    /// <summary>
    /// Конструктор системы управления целями.
    /// </summary>
    /// <param name="tower">Ссылка на поведение башни-владельца.</param>
    /// <param name="priorityMode">Начальный режим приоритета целей.</param>
    /// <param name="range">Радиус поиска целей.</param>
    public TowerTargetSystem(TowerBehaviour tower, TargetPriorityMode priorityMode, float range)
    {
        this.tower = tower;
        this.currentPriorityMode = priorityMode;
        this.acquisitionRange = range;
    }

    /// <summary>
    /// Поиск новой цели согласно выбранному режиму приоритета.
    /// Вызывается когда текущая цель потеряна или не найдена.
    /// </summary>
    /// <returns>true - если цель найдена, false - если нет подходящих целей.</returns>
    public bool FindNewTarget()
    {
        List<EnemyBehaviour> enemiesInRange = FindAllEnemiesInRange();

        if (enemiesInRange.Count == 0)
        {
            currentTarget = null;
            return false;
        }

        currentTarget = SelectTargetByPriority(enemiesInRange);
        return currentTarget != null;
    }

    /// <summary>
    /// Проверяет, остается ли текущая цель валидной.
    /// Цель считается невалидной если уничтожена или вышла за пределы радиуса атаки.
    /// </summary>
    /// <returns>true - если цель валидна, false - если требуется поиск новой цели.</returns>
    public bool IsCurrentTargetValid()
    {
        if (currentTarget == null) return false;

        // Проверяем что объект врага не уничтожен и что цель находится в радиусе атаки
        return currentTarget.gameObject != null &&
               Vector3.Distance(tower.transform.position, currentTarget.transform.position) <= acquisitionRange;
    }

    /// <summary>
    /// Смена режима приоритета целей.
    /// При смене режима текущая цель сбрасывается для применения нового алгоритма.
    /// </summary>
    /// <param name="newMode">Новый режим приоритета.</param>
    public void SetPriorityMode(TargetPriorityMode newMode)
    {
        currentPriorityMode = newMode;
        currentTarget = null; // Сброс цели для применения нового алгоритма
    }

    /// <summary>
    /// Обновление радиуса поиска целей.
    /// Вызывается при изменении характеристик башни (исследования, улучшения).
    /// </summary>
    /// <param name="newRange">Новый радиус поиска.</param>
    public void UpdateTowerRange(float newRange)
    {
        acquisitionRange = newRange;

        // При изменении радиуса проверяем валидность текущей цели
        if (!IsCurrentTargetValid())
        {
            currentTarget = null;
        }
    }

    /// <summary>
    /// Поиск всех врагов в радиусе действия башни.
    /// Использует Physics.OverlapSphere для обнаружения коллайдеров врагов.
    /// </summary>
    /// <returns>Список всех найденных врагов в радиусе.</returns>
    private List<EnemyBehaviour> FindAllEnemiesInRange()
    {
        List<EnemyBehaviour> enemies = new List<EnemyBehaviour>();
        Collider[] collidersInRange = Physics.OverlapSphere(tower.transform.position, acquisitionRange);

        foreach (Collider collider in collidersInRange)
        {
            EnemyBehaviour enemy = collider.GetComponent<EnemyBehaviour>();

            if (enemy != null && enemy.gameObject != null)
            {
                enemies.Add(enemy);
            }
        }

        return enemies;
    }

    /// <summary>
    /// Выбор цели из списка врагов согласно текущему режиму приоритета.
    /// </summary>
    /// <param name="enemies">Список врагов в радиусе действия.</param>
    /// <returns>Выбранная цель или null если список пуст.</returns>
    private EnemyBehaviour SelectTargetByPriority(List<EnemyBehaviour> enemies)
    {
        switch (currentPriorityMode)
        {
            case TargetPriorityMode.Closest:
                return GetClosestEnemy(enemies);

            case TargetPriorityMode.Priority:
                return GetPriorityEnemy(enemies);

            default:
                Debug.LogWarning($"Неизвестный режим приоритета: {currentPriorityMode}. Используется режим по умолчанию.");
                return GetClosestEnemy(enemies);
        }
    }

    /// <summary>
    /// Алгоритм поиска ближайшего врага.
    /// Выбирает врага с минимальным расстоянием до башни.
    /// </summary>
    /// <param name="enemies">Список врагов для анализа.</param>
    /// <returns>Ближайший враг или null если список пуст.</returns>
    private EnemyBehaviour GetClosestEnemy(List<EnemyBehaviour> enemies)
    {
        EnemyBehaviour closest = null;
        float closestDistance = float.MaxValue;
        Vector3 towerPosition = tower.transform.position;

        foreach (EnemyBehaviour enemy in enemies)
        {
            float distance = Vector3.Distance(towerPosition, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    /// <summary>
    /// Алгоритм поиска приоритетной цели (заглушка для будущей реализации).
    /// В текущей реализации возвращает ближайшего врага.
    /// </summary>
    /// <param name="enemies">Список врагов для анализа.</param>
    /// <returns>Приоритетный враг или null если список пуст.</returns>
    private EnemyBehaviour GetPriorityEnemy(List<EnemyBehaviour> enemies)
    {
        // TODO: Реализовать логику приоритетных целей на основе типа врага, здоровья, скорости и т.д.
        Debug.LogWarning("Режим приоритетных целей еще не реализован. Используется ближайшая цель.");
        return GetClosestEnemy(enemies);
    }
}