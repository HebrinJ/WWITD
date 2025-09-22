using System;
using UnityEngine;

/// <summary>
/// Компонент, представляющий главное здание (штаб) игрока на тактическом уровне.
/// Отслеживает здоровье штаба, обрабатывает получаемый урон и уведомляет о своем уничтожении,
/// что приводит к поражению на уровне.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является главной целью уровня. Его уничтожение означает немедленное поражение.
/// Не содержит сложной логики, только отслеживание здоровья и уведомление о его изменении.</para>
/// <para><b>Критическая важность:</b> Напрямую связан с <see cref="LevelManager"/>, который подписывается на
/// событие <see cref="OnHeadquartersDestroyed"/> для обработки условия поражения.</para>
/// </remarks>
public class Headquarters : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Максимальное количество здоровья штаба. Устанавливается индивидуально для каждого уровня.")]
    private int maxHealth = 100;

    /// <summary>
    /// Текущее количество здоровья штаба. Изменяется при получении урона.
    /// При достижении нуля штаб считается уничтоженным.
    /// </summary>
    public int CurrentHealth { get; private set; }

    /// <summary>
    /// Событие, вызываемое при любом изменении здоровья штаба.
    /// Передает новое текущее значение здоровья.
    /// Используется для обновления UI (полоска здоровья).
    /// </summary>
    public event Action<int> OnHealthChanged;

    /// <summary>
    /// Событие, вызываемое при уничтожении штаба (здоровье достигло нуля).
    /// Является критическим сигналом поражения на уровне.
    /// Основной подписчик: <see cref="LevelManager"/>.
    /// </summary>
    public event Action OnHeadquartersDestroyed;

    /// <summary>
    /// Инициализация штаба при старте уровня. Устанавливает начальное здоровье.
    /// </summary>
    private void Start()
    {
        CurrentHealth = maxHealth;
        // Немедленно уведомляем UI о начальном здоровье
        OnHealthChanged?.Invoke(CurrentHealth);
        Debug.Log($"[Headquarters] Штаб инициализирован. Здоровье: {CurrentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Основной метод для нанесения урона штабу. Вызывается извне (например, из LevelManager при достижении врагом базы).
    /// </summary>
    /// <param name="damageAmount">Количество урона для нанесения.</param>
    public void TakeDamage(int damageAmount)
    {
        // Защита от некорректных значений и повторного урона после уничтожения
        if (damageAmount <= 0 || CurrentHealth <= 0)
            return;

        CurrentHealth -= damageAmount;
        // Гарантируем, что здоровье не станет отрицательным
        CurrentHealth = Mathf.Max(0, CurrentHealth);

        Debug.Log($"[Headquarters] Получено {damageAmount} урона! Текущее здоровье: {CurrentHealth}");

        // Уведомляем все заинтересованные системы об изменении здоровья
        OnHealthChanged?.Invoke(CurrentHealth);

        // Проверяем, не уничтожен ли штаб
        if (CurrentHealth <= 0)
        {
            DestroyHeadquarters();
        }
    }

    /// <summary>
    /// Внутренний метод, обрабатывающий логику уничтожения штаба.
    /// Вызывается когда здоровье достигает нуля.
    /// </summary>
    private void DestroyHeadquarters()
    {
        Debug.Log("[Headquarters] ШТАБ УНИЧТОЖЕН! ПОРАЖЕНИЕ!");

        // Оповещаем о поражении
        OnHeadquartersDestroyed?.Invoke();

        // Визуально деактивируем объект (можно заменить на анимацию разрушения)
        gameObject.SetActive(false);
    }
}