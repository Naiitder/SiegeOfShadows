using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    
    [SerializeField] private List<EnemyMovement> enemies = new List<EnemyMovement>();

    public Grid grid;
    public float steering = 10f;
    public float stopRadius = 0.5f;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        
        if (enemies.Count == 0)
            enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None).ToList();

        foreach (var em in enemies) if (em) em.Initialize();
        
        grid = GetComponent<Grid>();
    }
    
    void Update()
    {
        HandleEnemiesMovement();
    }

    void HandleEnemiesMovement()
    {
        if (grid == null || grid.target == null) return;
        foreach (var enemy in enemies)
        {
            Vector2 ePos = enemy.transform.position;

            float distToTarget = Vector2.Distance(ePos, (Vector2)grid.target.position);
            if (distToTarget < stopRadius)
                continue;

            Node node = grid.NodeFromWorldPoint(ePos);
            Vector2 dir = node.bestDirection;
            if (dir == Vector2.zero)
                dir = ((Vector2)grid.target.position - ePos).normalized;
            
            enemy.transform.position = ePos+dir * enemy.moveSpeed * Time.deltaTime;
        }
    }
    
    public void RegisterEnemy(EnemyMovement em)
    {
        enemies.Add(em);
        em.Initialize();
    }

    public void UnregisterEnemy(EnemyMovement em)
    {
        enemies.Remove(em);
    }

    public bool IsInList(EnemyMovement em)
    {
        return enemies.Contains(em);
    }
    
    
}
