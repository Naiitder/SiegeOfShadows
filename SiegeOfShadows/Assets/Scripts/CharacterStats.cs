using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private int currentHealth, maxHealth;
    private int armor;
    private int damage;
    private float moveSpeed;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public Action OnTakeDamage { get; set; }
    public Action OnDeath { get; set; }

    public void TakeDamage(int dmg)
    {
        if(currentHealth <= 0) return;
        
        currentHealth -= dmg;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath?.Invoke();
        }
        
        OnTakeDamage?.Invoke();
    }
}
