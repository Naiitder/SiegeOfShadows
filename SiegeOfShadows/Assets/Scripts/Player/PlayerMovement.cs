using System;
using UnityEngine;

public class PlayerMovement : CharacterMovement
{
    public EquippedAbilitySlot[] upgrades = new EquippedAbilitySlot[8];
    private Vector2 lastMoveDirection = new Vector2(-1, 0);
    
    protected override void Awake()
    {
        base.Awake();
        
        Stats = GetComponent<PlayerStats>();
        Stats.OnDeath += Die;

        foreach (var slot in upgrades)
        {
            if (slot == null || slot.ability == null) continue;

            if (slot.ability is PassiveAbility)
            {
                PassiveAbility passive = slot.ability as PassiveAbility;
                passive.Initialize(slot.level);
            }
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleAbilities();
    }

    private void HandleMovement()
    {
        if (InputController.instance == null) return;
        
        Vector2 input = InputController.instance.moveInput;
        Vector2 dir = input.sqrMagnitude > 1f ? input.normalized : input;
        
        rb.linearVelocity = dir * moveSpeed;

        UpdateAnimation();
    }
    
    private void HandleAbilities()
    {
        if (InputController.instance.moveInput != Vector2.zero)
        {
            lastMoveDirection = InputController.instance.moveInput.normalized;
        }

        Vector2 lookDirection = lastMoveDirection.normalized;
        
        foreach (var slot in upgrades)
        {
            if (slot == null || slot.ability == null) continue;

            if (slot.ability is ProjectileAbility)
            {
                slot.cooldownTimer -= Time.deltaTime;

                if (slot.cooldownTimer <= 0)
                {
                    ProjectileAbility projectileAbility = slot.ability as ProjectileAbility;
                    projectileAbility.InstantiateProjectile(transform, lookDirection, slot.level);
                    slot.cooldownTimer = slot.ability.GetCooldown(slot.level);
                }
            }
        }
    }

    private void Die()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Stats.TakeDamage(other.GetComponent<CharacterStats>().Damage);
        }
    }
}
