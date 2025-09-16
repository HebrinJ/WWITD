using System.Collections;
using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
    [Header("Tower Parameters")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private TowerDataSO data;
    private EnemyBehaviour currentTarget;
    private Coroutine attackCoroutine;
    private bool isActive = true;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
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

    public void SetData(TowerDataSO data)
    {
        this.data = data;
        StartAttackRoutine();
    }

    // Запускаем корутину атаки
    private void StartAttackRoutine()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    // Корутина отвечает за периодическую атаку
    private IEnumerator AttackRoutine()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(1f / data.attacksPerSecond);

            if (FindTarget())
            {
                Attack();
            }
        }
    }

    private bool FindTarget()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, data.baseRange);

        foreach (Collider enemyCollider in enemiesInRange)
        {
            EnemyBehaviour enemy = enemyCollider.GetComponent<EnemyBehaviour>();
            if (enemy != null)
            {
                currentTarget = enemy;
                return true;
            }
        }

        currentTarget = null;
        return false;
    }

    private void Attack()
    {
        if (currentTarget == null) return;

        GameObject projectileObject = Instantiate(data.projectilePrefab.gameObject,
                                                projectileSpawnPoint.position,
                                                projectileSpawnPoint.rotation);

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.SetTarget(currentTarget.transform, data.baseDamage);
        }
        else
        {
            Debug.LogError("Projectile prefab doesn't have Projectile component!");
            Destroy(projectileObject);
        }
    }

    // Визуализация радиуса в редакторе
    private void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.baseRange);
        }
    }

    private void OnDestroy()
    {
        isActive = false;
        if (this != null && attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
    }
}