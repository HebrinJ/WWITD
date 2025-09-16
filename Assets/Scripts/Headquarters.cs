using System;
using UnityEngine;

public class Headquarters : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public int CurrentHealth { get; private set; }

    public event Action<int> OnHealthChanged;
    public event Action OnHeadquartersDestroyed;

    private void Start()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth);
        Debug.Log($"Headquarters initialized. Health: {CurrentHealth}/{maxHealth}");
    }

    public void TakeDamage(int damageAmount)
    {
        if (damageAmount <= 0 || CurrentHealth <= 0)
            return;

        CurrentHealth -= damageAmount;
        // Ограничиваем здоровье нулем снизу, чтобы не уйти в минус.
        CurrentHealth = Mathf.Max(0, CurrentHealth);

        Debug.Log($"Headquarters took {damageAmount} damage! Current health: {CurrentHealth}");

        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0)
        {
            DestroyHeadquarters();
        }
    }

    private void DestroyHeadquarters()
    {
        Debug.Log("HEADQUARTERS DESTROYED! GAME OVER!");

        OnHeadquartersDestroyed?.Invoke();

        gameObject.SetActive(false);
    }


} 


