using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Health : MonoBehaviour
{   
    public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }

    //This sets the healh when the game starts 
    public void SetInitialHealth(float MaxHealth)
    {
        this.MaxHealth = MaxHealth;
        CurrentHealth = MaxHealth;
    }

    public void HealObject(float healAmount)
    {
        CurrentHealth += healAmount;
        checkOverHeal();
    }

    private void checkOverHeal()
    {
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
    }

    public void DealDamage(float damage)
    {
        CurrentHealth -= damage;
    }
}
