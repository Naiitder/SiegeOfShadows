using System;
using UnityEngine;

public class EnemyMovement : CharacterMovement
{
    private Transform player;

    public void Initialize(Transform playerTarget)
    {
        this.player = playerTarget;
    }
    
    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
            
            UpdateAnimation();
        }
    }
}
