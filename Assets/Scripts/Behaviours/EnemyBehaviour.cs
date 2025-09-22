using UnityEngine;

/// <summary>
/// �����, ����������� ���������� � ���������� ���������� ����� �� ����������� ������.
/// �������� �� �������� �� ����, ��������� �����, ��������� ����� ���� ��� ���������� � ����������� � ����� ��������� (������, ���������� ����).
/// </summary>
/// <remarks>
/// <para><b>���� � �����������:</b> �������� "������������" ������ �� <see cref="EnemyDataSO"/>.
/// �� ��������� �������������� �������, ������ ��������� ���������������� ��������� �� ������ ����� ������.</para>
/// <para><b>��������� ����:</b> ��������� <see cref="WaveManager"/>, ���������������� �������, �������� �� ����,
/// ���� ��������� ���� (������� ����), ���� ������������ ������� (���� �������).</para>
/// </remarks>
public class EnemyBehaviour : MonoBehaviour
{
    /// <summary>
    /// ������ ����� (ScriptableObject), ���������� ��� ��������������: ��������, ����, ��������, ������� � �.�.
    /// ��������������� ����� ����� <see cref="SetData"/> ����� �������� ����������.
    /// </summary>
    private EnemyDataSO data;

    /// <summary>
    /// ������� ���������� �������� �����. ����������� ��� ��������� �����.
    /// </summary>
    private int currentHealth;

    /// <summary>
    /// ������� ������� ����� ����, � ������� �������� ����.
    /// </summary>
    private Transform targetWaypoint;

    /// <summary>
    /// ������ ������� ������� ����� � ������� Waypoints.Points.
    /// </summary>
    private int currentWaypointIndex = 0;

    /// <summary>
    /// ��������� �������� ������ ��� ������, ��������������� ������ � ������ �����.
    /// ������������ ������� ��������� (��������, ��� ����������� ���� ����� ��� �������).
    /// </summary>
    public EnemyDataSO Data => data;

    /// <summary>
    /// ����� ������������� �����. ������ ���������� ����� ����� �������� ����������.
    /// </summary>
    /// <param name="data">ScriptableObject � ������� �����, ���������� ��� ��� ��������������.</param>
    public void SetData(EnemyDataSO data)
    {
        this.data = data;
        currentHealth = data.maxHealth;
    }

    /// <summary>
    /// ������������� ���� �������� ��� �����. ������ ���������� ����� SetData.
    /// </summary>
    /// <param name="waypoints">������ Transform'��, �������������� ����� ����.</param>
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
    /// �������� ����� Update, ���������� ������ ����. �������� �� �������� ����� �� ����.
    /// </summary>
    void Update()
    {
        // �������� � ������� ������� �����
        if (targetWaypoint != null)
        {
            // ������� �������� towards ������� ����� � ������ �������� �� ������
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, data.moveSpeed * Time.deltaTime);

            // ���������, ����� �� �� ������� ����� (� ��������� ��������)
            if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
            {
                GetNextWaypoint();
            }
        }
    }

    /// <summary>
    /// ���������� ����� ��� �������� � ��������� ����� ����.
    /// ���� ����� ������ ���, ���� ��������� ����.
    /// </summary>
    private void GetNextWaypoint()
    {
        currentWaypointIndex++;

        // ���������, �� �������� �� �� ����� ����
        if (currentWaypointIndex >= Waypoints.Instance.PathPoints.Length)
        {
            ReachedBase();
            return;
        }

        // ������������� ��������� ����� ��� ����
        targetWaypoint = Waypoints.Instance.PathPoints[currentWaypointIndex];
    }

    /// <summary>
    /// �����, ���������� ����� ���� ������ �������� ����� ���� (����).
    /// ��������� ������� � ��������� ����� ���� � ���������� ����.
    /// </summary>
    private void ReachedBase()
    {
        Debug.Log($"[Enemy] {data.enemyName} ������ ����! ������� {data.damageToBase} �����.");

        // ��������� EventHub � ���, ��� ���� ����� �� ���� � ����� ������� ����.
        // �������� ������ �� ����, ����� ������� �����, ����� ������ ���� � ����� ���� �������.
        EventHub.OnEnemyReachedBase?.Invoke(this);

        // ���� �������� ����� ���������� ���� (��������� "�������" ��)
        Destroy(gameObject);
    }

    /// <summary>
    /// ��������� ����� ��� ��������� ����� �����. ���������� ����� (��������, �� TowerBehaviour ��� ���������).
    /// </summary>
    /// <param name="damage">���������� ����� ��� ���������.</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // ���������� ��� �������� �������� ����� � ��������� ����� ����� ���� ��������� �����

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// �����, ���������� ��� ���������� ��������� ����.
    /// ��������� ������� � ������ ����� (��� �������, ����������) � ���������� ������.
    /// </summary>
    private void Die()
    {
        Debug.Log($"[Enemy] {data.enemyName} ���������! �������: {data.rewardMoney}.");

        // ��������� � ������ ��� ���������� �������� �������
        EventHub.OnEnemyDiedForMoney?.Invoke(this);

        // ��������� � ������ ��� ����� ���� (����������, ����������, WaveManager)
        EventHub.OnEnemyDied?.Invoke(this);

        Destroy(gameObject);
    }
}