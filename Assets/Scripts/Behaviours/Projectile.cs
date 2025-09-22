using UnityEngine;

/// <summary>
/// Компонент, управляющий поведением снаряда (пули, снаряда), выпущенного башней.
/// Отвечает за перемещение к цели, нанесение урона при попадании и самоуничтожение.
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является "исполнителем" выстрела башни. Создается башней (<see cref="TowerBehaviour"/>),
/// летит к цели (<see cref="EnemyBehaviour"/>) и наносит урон. Не содержит сложной логики принятия решений.</para>
/// <para><b>Жизненный цикл:</b> Создается -> Нацеливается -> Движется -> Попадает -> Уничтожается.</para>
/// </remarks>
public class Projectile : MonoBehaviour
{
    /// <summary>
    /// Количество урона, которое нанесет снаряд при попадании.
    /// Устанавливается башней при создании снаряда.
    /// </summary>
    private int damage;

    /// <summary>
    /// Цель снаряда (враг). Снаряд будет двигаться к этой цели до попадания или ее исчезновения.
    /// </summary>
    private Transform target;

    /// <summary>
    /// Скорость движения снаряда в единицах в секунду.
    /// </summary>
    private float speed = 15f;

    /// <summary>
    /// Метод инициализации снаряда. Должен вызываться сразу после создания экземпляра.
    /// </summary>
    /// <param name="target">Transform цели (врага), в которую летит снаряд.</param>
    /// <param name="damage">Количество урона, которое нанесет снаряд при попадании.</param>
    public void SetTarget(Transform target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }

    /// <summary>
    /// Основной метод Update, вызываемый каждый кадр. Отвечает за движение снаряда к цели.
    /// </summary>
    void Update()
    {
        // Если цель уничтожена до попадания - уничтожаем и снаряд
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Расчет направления движения к цели
        Vector3 direction = (target.position - transform.position).normalized;

        // Перемещение снаряда с постоянной скоростью
        transform.position += direction * speed * Time.deltaTime;

        // Поворот снаряда в направлении движения для визуальной правдоподобности
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Проверка достижения цели (попадания)
        if (Vector3.Distance(transform.position, target.position) < 0.3f)
        {
            HitTarget();
        }
    }

    /// <summary>
    /// Метод, вызываемый при достижении снарядом цели. Наносит урон врагу и уничтожает снаряд.
    /// </summary>
    private void HitTarget()
    {
        if (target != null)
        {
            // Пытаемся получить компонент EnemyBehaviour у цели
            EnemyBehaviour enemy = target.GetComponent<EnemyBehaviour>();
            if (enemy != null)
            {
                // Наносим урон врагу
                enemy.TakeDamage(damage);

                // Здесь можно добавить визуальные/звуковые эффекты попадания
            }
        }

        // Уничтожаем снаряд после попадания
        Destroy(gameObject);
    }

    // NOTE: Для будущего развития можно добавить:
    // - Поддержку различных типов снарядов (скорость, траектория, визуал)
    // - Эффекты нанесения урона (взрыв, огонь, замедление)
    // - Систему объектного пулинга для оптимизации
}