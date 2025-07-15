using System;
using UnityEngine;

public class EnemyMovement : CharacterMovement
{
    private Transform _player;
    [SerializeField] private LayerMask layerMask;

    public void Initialize(Transform playerTarget)
    {
        this._player = playerTarget;
    }
    
    public void HandleMovement()
    {
        if (_player == null) return;

        Vector2 direction = (_player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1f, layerMask);

        if (hit.collider == null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            Vector2 perpDirection = Vector2.Perpendicular(direction);
            Vector2 newDirection = perpDirection; 
            rb.linearVelocity = newDirection * moveSpeed * 0.5f; 
        }

        UpdateAnimation();
    }
}
