using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
    [Header("Tower Parameters")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private TowerDataSO data;
    private EnemyBehaviour currentTarget;
    private Coroutine attackCoroutine;
    private bool isActive = true;

    private Dictionary<string, int> flatBonuses = new Dictionary<string, int>();
    private Dictionary<string, float> multiplicativeBonuses = new Dictionary<string, float>();

    private int Damage;
    private float baseFireRate;
    private float baseFireDistance;
    private int baseHealth;
    private int baseArmor;

    private void Start()
    {
        Damage = data.damage;
        baseFireRate = data.fireRate;
        baseFireDistance = data.fireDistance;
        baseHealth = data.maxHealth;
        baseArmor = data.maxArmor;

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

    // Методы для получения модифицированных значений с абсолютными бонусами
    public int GetModifiedDamage()
    {
        int flatBonus = flatBonuses.ContainsKey("Damage") ? flatBonuses["Damage"] : 0;
        float multiplier = multiplicativeBonuses.ContainsKey("Damage") ? multiplicativeBonuses["Damage"] : 1f;

        return Mathf.RoundToInt((Damage + flatBonus) * multiplier);
    }

    public float GetModifiedFireRate()
    {
        float flatBonus = flatBonuses.ContainsKey("FireRate") ? flatBonuses["FireRate"] : 0f;
        float multiplier = multiplicativeBonuses.ContainsKey("FireRate") ? multiplicativeBonuses["FireRate"] : 1f;

        return (baseFireRate + flatBonus) * multiplier;
    }

    public float GetModifiedFireDistance()
    {
        float flatBonus = flatBonuses.ContainsKey("FireDistance") ? flatBonuses["FireDistance"] : 0f;
        float multiplier = multiplicativeBonuses.ContainsKey("FireDistance") ? multiplicativeBonuses["FireDistance"] : 1f;

        return (baseFireDistance + flatBonus) * multiplier;
    }

    public int GetModifiedHealth()
    {
        int flatBonus = flatBonuses.ContainsKey("Health") ? flatBonuses["Health"] : 0;
        float multiplier = multiplicativeBonuses.ContainsKey("Health") ? multiplicativeBonuses["Health"] : 1f;

        return Mathf.RoundToInt((baseHealth + flatBonus) * multiplier);
    }

    public int GetModifiedArmor()
    {
        int flatBonus = flatBonuses.ContainsKey("Armor") ? flatBonuses["Armor"] : 0;
        float multiplier = multiplicativeBonuses.ContainsKey("Armor") ? multiplicativeBonuses["Armor"] : 1f;

        return Mathf.RoundToInt((baseArmor + flatBonus) * multiplier);
    }

    // Методы для применения бонусов
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
        LogCurrentStats(); // Логируем текущие характеристики
    }

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
        LogCurrentStats(); // Логируем текущие характеристики
    }

    // Метод для логирования текущих характеристик башни
    public void LogCurrentStats()
    {
        Debug.Log($"Tower {data.towerName} stats:\n" +
                 $"Damage: {GetModifiedDamage()} (base: {Damage} + flat: {GetFlatBonus("Damage")} * mult: {GetMultiplier("Damage")})\n" +
                 $"Fire Rate: {GetModifiedFireRate():F2} (base: {baseFireRate} + flat: {GetFlatBonus("FireRate")} * mult: {GetMultiplier("FireRate")})\n" +
                 $"Range: {GetModifiedFireDistance():F1} (base: {baseFireDistance} + flat: {GetFlatBonus("FireDistance")} * mult: {GetMultiplier("FireDistance")})");
    }

    private int GetFlatBonus(string statName)
    {
        return flatBonuses.ContainsKey(statName) ? flatBonuses[statName] : 0;
    }

    private float GetMultiplier(string statName)
    {
        return multiplicativeBonuses.ContainsKey(statName) ? multiplicativeBonuses[statName] : 1f;
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
            yield return new WaitForSeconds(1f / data.fireRate);

            if (FindTarget())
            {
                Attack();
            }
        }
    }

    private bool FindTarget()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, data.fireDistance);

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

        // Используем модифицированный урон!
        int finalDamage = GetModifiedDamage();

        // ЛОГИРУЕМ УРОН В КОНСОЛЬ
        Debug.Log($"{data.towerName} attacks for {finalDamage} damage! " +
                 $"(Base: {Damage} + Flat: {GetFlatBonus("Damage")} * Multiplier: {GetMultiplier("Damage"):F2})");

        if (projectile != null)
        {
            projectile.SetTarget(currentTarget.transform, finalDamage);
        }
        else
        {
            Debug.LogError("Projectile prefab doesn't have Projectile component!");
            Destroy(projectileObject);
        }
    }

    public TowerType GetTowerType()
    {
        return data.towerType;
    }

    // Визуализация радиуса в редакторе
    private void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.fireDistance);
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