using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    
    [SerializeField] private List<EnemyMovement> enemies = new List<EnemyMovement>();
    private PlayerMovement player;
    [SerializeField] private FlowFieldGrid2D flow;
    [SerializeField] private float separationRadius = 0.5f;
    [SerializeField] private float separationWeight = 0.6f;
    [SerializeField] private float hashCellSize = 1.0f;

    private SpatialHash2D<EnemyMovement> hash;
    private readonly List<EnemyMovement> tmp = new();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        
        player = FindAnyObjectByType<PlayerMovement>();
        enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None).ToList();
        
        foreach (var em in enemies) if (em) em.Initialize(player.transform, flow);
        hash = new SpatialHash2D<EnemyMovement>(hashCellSize);
    }

    private void FixedUpdate()
    {
        hash.Clear();
        for (int i = 0; i < enemies.Count; i++)
        {
            var em = enemies[i];
            if (!em) continue;
            hash.Insert(em.transform.position, em);
        }
        
        for (int i = 0; i < enemies.Count; i++)
        {
            var em = enemies[i];
            if (!em) continue;

            Vector2 desired = flow ? flow.GetFlowDir(em.transform.position) : Vector2.zero;
            if (desired == Vector2.zero)
            {
                desired = ((Vector2)player.transform.position - (Vector2)em.transform.position).normalized;
            }
            
            hash.QueryRadius(em.transform.position, separationRadius, tmp, e => e.transform.position);
            Vector2 sep = Vector2.zero;
            for (int t = 0; t < tmp.Count; t++)
            {
                var other = tmp[t];
                if (other == em) continue;
                var delta = (Vector2)(em.transform.position - other.transform.position);
                float d2 = delta.sqrMagnitude + 1e-4f;
                sep += delta / d2;
            }

            em.HandleMovement(desired + separationWeight * sep);
        }

    }

    public void RegisterEnemy(EnemyMovement em)
    {
        enemies.Add(em);
    }

    public void UnregisterEnemy(EnemyMovement em)
    {
        enemies.Remove(em);
    }

    public bool IsInList(EnemyMovement em)
    {
        return enemies.Contains(em);
    }
    
    public EnemyMovement GetClosestEnemy(Vector2 origin, float maxDistance = Mathf.Infinity)
    {
        EnemyMovement closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (EnemyMovement enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(origin, enemy.transform.position);

            if (distance < closestDistance && distance <= maxDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }
    
    public void GetEnemiesInRadius(Vector2 center, float radius, List<EnemyMovement> outList)
    {
        hash.QueryRadius(center, radius, outList, e => e.transform.position);
    }

}
