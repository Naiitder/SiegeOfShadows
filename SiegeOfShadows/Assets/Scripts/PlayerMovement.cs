using System;
using UnityEngine;

public class PlayerMovement : CharacterMovement
{
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
}
