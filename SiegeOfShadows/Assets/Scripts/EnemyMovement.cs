using UnityEngine;

public class EnemyMovement : CharacterMovement
{

    protected override void Awake()
    {
        base.Awake();
        stats.OnDeath += Die;
        if(EnemyManager.instance != null) Initialize();
    }

    public void Initialize()
    {
        if(!EnemyManager.instance.IsInList(this)) EnemyManager.instance.RegisterEnemy(this);
    }
    
    private void Die()
    {
        Destroy(this.gameObject);
    }
    
    
    private void OnDestroy()
    {
        if(EnemyManager.instance.IsInList(this)) EnemyManager.instance.UnregisterEnemy(this);
    }

}
