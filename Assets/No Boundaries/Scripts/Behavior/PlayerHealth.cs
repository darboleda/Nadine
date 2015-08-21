using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public int MaxHealth;
    public int CurrentHealth;

    public event Action<int, int> HealthChanged;

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        NotifyHealthChanged();
    }

    public void TakeDamage(int damage, int minHealthLeft)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - damage, minHealthLeft);
        NotifyHealthChanged();
    }
    
    public void Heal(int amountToHeal)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amountToHeal, MaxHealth);
        NotifyHealthChanged();
    }

    public void SetMaxHealth(int newValue)
    {
        MaxHealth = newValue;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        NotifyHealthChanged(CurrentHealth, MaxHealth);
    }

    private void NotifyHealthChanged(int current, int max)
    {
        if (HealthChanged != null)
        {
            HealthChanged(current, max);
        }
    }
}
