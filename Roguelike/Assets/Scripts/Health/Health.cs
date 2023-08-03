using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Health : MonoBehaviour
{

    [SerializeField] int _maxHealth;

    public int maxHealth
    {
        get { return _maxHealth; }

        set { _maxHealth = value; }
    }
    
    [SerializeField] int _health; 

    public int health 
    {
        get { return _health; }
        set { _health = value;}
    }

    //This sets the healh when the game starts 
    public void SetInitialHealth(int maxHealth)
    {
        this.maxHealth = maxHealth;
        health = maxHealth;
    }

}
