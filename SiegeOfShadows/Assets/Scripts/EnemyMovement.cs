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
    if (_flow == null || !_player) return;
    
    Vector2 dir = desired.sqrMagnitude > 1e-6f ? desired.normalized : Vector2.zero;
    float speed = moveSpeed;
    
    float maxStep = _flow != null ? _flow.cellSize * 0.45f : 0.25f;
    Vector2 total = dir * speed * Time.deltaTime;
    int steps = Mathf.Max(1, Mathf.CeilToInt(total.magnitude / maxStep));
    Vector2 step = (steps > 0) ? total / steps : Vector2.zero;

    Vector2 pos = rb.position;
    Vector2 startPos = pos;

    for (int i = 0; i < steps; i++)
    {
        Vector2 tryPos = pos + step;

        if (!_flow.IsBlocked(tryPos))
        {
            Debug.DrawLine(pos, tryPos, Color.green, 0.05f);
            pos = tryPos;
        }
        else
        {
            Vector2 tryX = new Vector2(pos.x + step.x, pos.y);
            Vector2 tryY = new Vector2(pos.x, pos.y + step.y);

            bool moved = false;
            if (!_flow.IsBlocked(tryX))
            {
                Debug.DrawLine(pos, tryX, Color.yellow, 0.05f);
                pos.x = tryX.x;
                moved = true;
            }
            if (!_flow.IsBlocked(tryY))
            {
                Debug.DrawLine(new Vector2(pos.x, pos.y), tryY, Color.cyan, 0.05f);
                pos.y = tryY.y;
                moved = true;
            }

            if (!moved)
            {
                Debug.DrawLine(pos, tryPos, Color.red, 0.1f);
                break;
            }
        }
    }

    rb.MovePosition(pos);
    
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
