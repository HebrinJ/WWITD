using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int damage;
    private Transform target;
    private float speed = 15f;

    public void SetTarget(Transform target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // ������� �������� � ����
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // ������� � ������� ����
        transform.rotation = Quaternion.LookRotation(direction);

        // �������� ���������� ����
        if (Vector3.Distance(transform.position, target.position) < 0.3f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (target != null)
        {
            EnemyBehaviour enemy = target.GetComponent<EnemyBehaviour>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }
}