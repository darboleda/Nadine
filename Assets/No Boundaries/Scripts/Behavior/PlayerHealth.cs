using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public string DisplayId;

    public int MaxHealth;
    public int CurrentHealth;

    public void OnEnable()
    {
        UpdateDisplay();
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        UpdateDisplay();
    }

    public void TakeDamage(int damage, int minHealthLeft)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - damage, minHealthLeft);
        UpdateDisplay();
    }
    
    public void Heal(int amountToHeal)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amountToHeal, MaxHealth);
        UpdateDisplay();
    }

    public void SetMaxHealth(int newValue)
    {
        MaxHealth = newValue;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        foreach (CountDisplay display in Hud.FindHud().RequestDisplay<CountDisplay>(DisplayId))
        {
            display.Set(CurrentHealth, MaxHealth);
        }
    }
}
