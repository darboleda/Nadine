using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    public PlayerHealth PlayerHealth;
    public HealthDisplay PlayerHealthDisplay;

    public void Start()
    {
        PlayerHealth.HealthChanged += UpdatePlayerHealthDisplay;
        UpdatePlayerHealthDisplay(PlayerHealth.CurrentHealth, PlayerHealth.MaxHealth);
    }

/*
    public void Update()
    {
        UpdatePlayerHealthDisplay(PlayerHealth.CurrentHealth, PlayerHealth.MaxHealth);
    }
*/

    public void UpdatePlayerHealthDisplay(int current, int max)
    {
        PlayerHealthDisplay.SetHealth(current, max);
    }
}
