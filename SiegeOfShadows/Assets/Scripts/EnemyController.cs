using UnityEngine;

public class EnemyController : CharacterMovement
{
    [Header("Collision Settings")]
    public float contactRadius = 0.45f;

    [Header("Drop settings")] 
    public GameObject expGameObject;
    public int expDrop = 5;
    public float prizeDropRate = 0.01f;
    
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
        SpawnReward();
        Destroy(this.gameObject);
    }
    
    
    private void OnDestroy()
    {
        if(EnemyManager.instance.IsInList(this)) EnemyManager.instance.UnregisterEnemy(this);
    }

    private void SpawnReward()
    {
        if (expGameObject == null) return;
        
        GameObject expObj = Instantiate(expGameObject, transform.position, Quaternion.identity);
        Experience exp = expObj.GetComponent<Experience>();
        if(exp == null) return;
        exp.Initialize(expDrop);
    }

}
