using System;
using UnityEngine;

public class PlayerMovement : CharacterMovement
{
    protected override void Awake()
    {
        base.Awake();
        Stats.OnDeath += Die;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (InputController.instance != null)
        {
            rb.linearVelocity = InputController.instance.moveInput * moveSpeed;
            
            UpdateAnimation();
        }
    }

    private void Die()
    {
        
    }
}
