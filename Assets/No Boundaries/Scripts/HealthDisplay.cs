using UnityEngine;

public abstract class HealthDisplay : MonoBehaviour
{
    private int maxHealth;
    private int currentHealth;

    public void SetCurrentHealth(int value)
    {
        currentHealth = value;
        UpdateHealth(currentHealth, maxHealth);
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
        UpdateHealth(currentHealth, maxHealth);
    }

    public void SetHealth(int current, int max)
    {
        currentHealth = current;
        maxHealth = max;
        UpdateHealth(current, max);
    }

    protected abstract void UpdateHealth(int current, int max);
}
