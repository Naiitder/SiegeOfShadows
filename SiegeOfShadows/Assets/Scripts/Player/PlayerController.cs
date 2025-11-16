using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CharacterMovement
{
    public EquippedAbilitySlot[] upgrades = new EquippedAbilitySlot[8];
    private Vector2 lastMoveDirection = new Vector2(-1, 0);
    
    private PlayerStats playerStats;
    
    public float pickupRadius = 0.4f;
    
    [SerializeField] protected Rigidbody2D rb;
    
    protected override void Awake()
    {
        base.Awake();
        
        playerStats = GetComponent<PlayerStats>();
        playerStats.Initialize();
        stats = playerStats; 
        
        rb = GetComponent<Rigidbody2D>();
        
        stats.OnDeath += Die;

        foreach (var slot in upgrades)
        {
            if (slot == null || slot.ability == null) continue;

            if (slot.ability is PassiveAbility)
            {
                PassiveAbility passive = slot.ability as PassiveAbility;
                if (passive != null) passive.Initialize(slot.level);
            }
        }
    }

    private void Update()
    {
        HandleAbilities();
        playerStats.UpdateEaseSliders();
    }

    private void FixedUpdate()
    {
        HandleMovement();
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
                    if (projectileAbility != null)
                        projectileAbility.InstantiateProjectile(transform, lookDirection, slot.level);
                    slot.cooldownTimer = slot.ability.GetCooldown(slot.level);
                }
            }
        }
    }

    private void Die()
    {
        
    }
    
    private void UpdateAnimation()
    {
        if(rb.linearVelocity.x < 0) SpriteRenderer.flipX = false;
        else if (rb.linearVelocity.x > 0) SpriteRenderer.flipX = true;
            
        if(rb.linearVelocity.magnitude > 0) Animator.SetBool(IsMovingHash, true);
        else Animator.SetBool(IsMovingHash, false);
    }
    
}
