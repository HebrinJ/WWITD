using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private EnemyDataSO data;
    private int currentHealth;
    private Transform targetWaypoint;
    private int currentWaypointIndex = 0;

    public EnemyDataSO Data => data;

    // ����� ��� ������������� ����� ������� �� SO
    public void SetData(EnemyDataSO data)
    {
        this.data = data;
        currentHealth = data.maxHealth;
    }

    // ��������������, ��� � ��� ���� ����� Waypoints �� ����������� �������� ����� ����
    public void SetPath(Transform[] waypoints)
    {
        targetWaypoint = waypoints[0];
    }

    void Update()
    {
        // �������� � �����
        if (targetWaypoint != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, data.moveSpeed * Time.deltaTime);

            // ���������, ����� �� �� ������� �����
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

        // ��������� EventHub � ���, ��� ���� ����� �� ���� � ����� ������� ����.
        EventHub.OnEnemyReachedBase?.Invoke(this);

        Destroy(gameObject);
    }

    // ����� ��� ��������� ����� (������� ��� ����� �� �����)
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
        // �������� � ������ ����� ����� EventHub
        EventHub.OnEnemyDied?.Invoke(this); // �������� ����, ����� ��������� �����, ��� ����
        Destroy(gameObject);
    }
}
