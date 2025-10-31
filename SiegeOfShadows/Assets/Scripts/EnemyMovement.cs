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


    public void ApplyJobPosition(Vector2 newPos, float dt, Vector2 desiredDir)
    {
        transform.position = newPos;

        Vector2 dir = desiredDir.sqrMagnitude > 1e-6f ? desiredDir : (newPos - lastPos);
        if (Mathf.Abs(dir.x) > 0.02f)
            SpriteRenderer.flipX = dir.x > 0f;

        lastPos = newPos;
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
