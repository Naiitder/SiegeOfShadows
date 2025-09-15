using System;
using UnityEngine;

public class EnemyMovement : CharacterMovement
{
    private Transform _player;
    private FlowFieldGrid2D _flow;

    protected override void Awake()
    {
        base.Awake();
        Stats.OnDeath += Die;
    }

    public void Initialize(Transform playerTarget, FlowFieldGrid2D flow)
    {
        _player = playerTarget;
        _flow = flow;
        if(!EnemyManager.instance.IsInList(this)) EnemyManager.instance.RegisterEnemy(this);
    }
    
    public void HandleMovement(Vector2 desired)
    {
        if (!_player) return;

        Vector2 v = desired.normalized * moveSpeed;
        
        if (_flow && _flow.IsBlocked(transform.position))
        {
            Vector2 toFree = (_player.position - transform.position).normalized;
            v += toFree * (moveSpeed * 0.5f);
        }
        
        rb.linearVelocity = v;

        UpdateAnimation();
    }

    private void OnDestroy()
    {
        if(EnemyManager.instance.IsInList(this)) EnemyManager.instance.UnregisterEnemy(this);
    }

    private void Die()
    {
        Destroy(this.gameObject);
    }
}
