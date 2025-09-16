using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private EnemyDataSO data;
    private int currentHealth;
    private Transform targetWaypoint;
    private int currentWaypointIndex = 0;

    public EnemyDataSO Data => data;

    // Метод для инициализации врага данными из SO
    public void SetData(EnemyDataSO data)
    {
        this.data = data;
        currentHealth = data.maxHealth;
    }

    // Предполагается, что у вас есть класс Waypoints со статическим массивом точек пути
    public void SetPath(Transform[] waypoints)
    {
        targetWaypoint = waypoints[0];
    }

    void Update()
    {
        // Движение к точке
        if (targetWaypoint != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, data.moveSpeed * Time.deltaTime);

            // Проверяем, дошли ли до текущей точки
            if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
            {
                GetNextWaypoint();
            }
        }
    }

    private void GetNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= Waypoints.Points.Length)
        {
            ReachedBase();
            return;
        }
        targetWaypoint = Waypoints.Points[currentWaypointIndex];
    }

    private void ReachedBase()
    {
        Debug.Log("Enemy reached the base!");

        // Оповещаем EventHub о том, что враг дошел до базы и готов нанести урон.
        EventHub.OnEnemyReachedBase?.Invoke(this);

        Destroy(gameObject);
    }

    // Метод для получения урона (вызовем его позже из башни)
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy died! Granting reward: " + data.rewardMoney);
        // Сообщаем о смерти врага через EventHub
        EventHub.OnEnemyDied?.Invoke(this); // Передаем себя, чтобы слушатели знали, кто умер
        Destroy(gameObject);
    }
}
