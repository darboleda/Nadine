using UnityEngine;
using System.Collections;

public class PlayerState : MonoBehaviour {

    public PlayerHealth Health;
    public CountDisplay HealthDisplay;

    public PlayerWallet Wallet;
    public CountDisplay MoneyDisplay;

    public CountDisplay KeyDisplay;

    private int count;

    public void OnEnable()
    {
        if (Health != null)
        {
            Health.HealthChanged += SetHealth;
            SetHealth(Health.CurrentHealth, Health.MaxHealth);
        }

        if (Wallet != null)
        {
            Wallet.AmountChanged += SetMoney;
            SetMoney(Wallet.Current, Wallet.Max);
        }
    }

    public void OnDisable()
    {
        if (Health != null)
        {
            Health.HealthChanged -= SetHealth;
        }

        if (Wallet != null)
        {
            Wallet.AmountChanged -= SetMoney;
        }
    }


    public void SetMoney(int value, int max)
    {
        MoneyDisplay.SetCurrent(value);
    }

    public void SetHealth(int current, int max)
    {
        HealthDisplay.Set(current, max);
    }

}
