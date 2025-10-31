using UnityEngine;

public class EnemyMovement : CharacterMovement
{

    [SerializeField] private float maxDtClamp = 1f / 30f;
    
    [Header("SpriteRenderer")]
    Vector2 lastPos;
    const float FlipDeadzone = 0.02f; 

    protected override void Awake()
    {
        base.Awake();
        Stats.OnDeath += Die;
    }

    public void Initialize()
    {
        if(!EnemyManager.instance.IsInList(this)) EnemyManager.instance.RegisterEnemy(this);
    }


public void ApplyJobPosition(Vector2 pos, float dt)
{
    transform.position = pos;
    
    Vector2 v = (pos - lastPos) / Mathf.Max(dt, 1e-6f);
    if (Mathf.Abs(v.x) > FlipDeadzone)
        SpriteRenderer.flipX = v.x > 0f;

    lastPos = pos;
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
