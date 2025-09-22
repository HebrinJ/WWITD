using UnityEngine;

/// <summary>
/// Класс, управляющий поведением и состоянием отдельного врага на тактическом уровне.
/// Отвечает за движение по пути, получение урона, нанесение урона базе при достижении и уведомление о своем состоянии (смерть, достижение базы).
/// </summary>
/// <remarks>
/// <para><b>Роль в архитектуре:</b> Является "исполнителем" данных из <see cref="EnemyDataSO"/>.
/// Не принимает стратегических решений, только выполняет предопределенное поведение на основе своих данных.</para>
/// <para><b>Жизненный цикл:</b> Создается <see cref="WaveManager"/>, инициализируется данными, движется по пути,
/// либо достигает базы (наносит урон), либо уничтожается башнями (дает награду).</para>
/// </remarks>
public class EnemyBehaviour : MonoBehaviour
{
    /// <summary>
    /// Данные врага (ScriptableObject), содержащие его характеристики: здоровье, урон, скорость, награду и т.д.
    /// Устанавливается через метод <see cref="SetData"/> после создания экземпляра.
    /// </summary>
    private EnemyDataSO data;

    /// <summary>
    /// Текущее количество здоровья врага. Уменьшается при получении урона.
    /// </summary>
    private int currentHealth;

    /// <summary>
    /// Текущая целевая точка пути, к которой движется враг.
    /// </summary>
    private Transform targetWaypoint;

    /// <summary>
    /// Индекс текущей целевой точки в массиве Waypoints.Points.
    /// </summary>
    private int currentWaypointIndex = 0;

    /// <summary>
    /// Публичное свойство только для чтения, предоставляющее доступ к данным врага.
    /// Используется другими системами (например, для определения типа урона или награды).
    /// </summary>
    public EnemyDataSO Data => data;

    /// <summary>
    /// Метод инициализации врага. Должен вызываться сразу после создания экземпляра.
    /// </summary>
    /// <param name="data">ScriptableObject с данными врага, содержащий все его характеристики.</param>
    public void SetData(EnemyDataSO data)
    {
        this.data = data;
        currentHealth = data.maxHealth;
    }

    /// <summary>
    /// Устанавливает путь движения для врага. Должен вызываться после SetData.
    /// </summary>
    /// <param name="waypoints">Массив Transform'ов, представляющих точки пути.</param>
    public void SetPath(Transform[] waypoints)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Waypoints array is null or empty!");
            return;
        }

        targetWaypoint = waypoints[0];
        currentWaypointIndex = 0;
    }

    /// <summary>
    /// Основной метод Update, вызываемый каждый кадр. Отвечает за движение врага по пути.
    /// </summary>
    void Update()
    {
        // Движение к текущей целевой точке
        if (targetWaypoint != null)
        {
            // Плавное движение towards целевой точки с учетом скорости из данных
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, data.moveSpeed * Time.deltaTime);

            // Проверяем, дошли ли до текущей точки (с небольшим допуском)
            if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
            {
                GetNextWaypoint();
            }
        }
    }

    /// <summary>
    /// Внутренний метод для перехода к следующей точке пути.
    /// Если точек больше нет, враг достигает базы.
    /// </summary>
    private void GetNextWaypoint()
    {
        currentWaypointIndex++;

        // Проверяем, не достигли ли мы конца пути
        if (currentWaypointIndex >= Waypoints.Instance.PathPoints.Length)
        {
            ReachedBase();
            return;
        }

        // Устанавливаем следующую точку как цель
        targetWaypoint = Waypoints.Instance.PathPoints[currentWaypointIndex];
    }

    /// <summary>
    /// Метод, вызываемый когда враг достиг конечной точки пути (базы).
    /// Оповещает систему о нанесении урона базе и уничтожает себя.
    /// </summary>
    private void ReachedBase()
    {
        Debug.Log($"[Enemy] {data.enemyName} достиг базы! Наносит {data.damageToBase} урона.");

        // Оповещаем EventHub о том, что враг дошел до базы и готов нанести урон.
        // Передаем ссылку на себя, чтобы система знала, какой именно враг и какой урон наносит.
        EventHub.OnEnemyReachedBase?.Invoke(this);

        // Враг исчезает после достижения базы (логически "атакует" ее)
        Destroy(gameObject);
    }

    /// <summary>
    /// Публичный метод для нанесения урона врагу. Вызывается извне (например, из TowerBehaviour при попадании).
    /// </summary>
    /// <param name="damage">Количество урона для нанесения.</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Визуальная или звуковая обратная связь о получении урона может быть добавлена здесь

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Метод, вызываемый при достижении здоровьем нуля.
    /// Оповещает системы о смерти врага (для награды, статистики) и уничтожает объект.
    /// </summary>
    private void Die()
    {
        Debug.Log($"[Enemy] {data.enemyName} уничтожен! Награда: {data.rewardMoney}.");

        // Оповещаем о смерти для начисления денежной награды
        EventHub.OnEnemyDiedForMoney?.Invoke(this);

        // Оповещаем о смерти для общих нужд (статистика, достижения, WaveManager)
        EventHub.OnEnemyDied?.Invoke(this);

        Destroy(gameObject);
    }
}