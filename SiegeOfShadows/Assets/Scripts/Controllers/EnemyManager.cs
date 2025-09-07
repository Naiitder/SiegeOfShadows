using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    
    [SerializeField] private List<EnemyMovement> enemies = new List<EnemyMovement>();
    private PlayerMovement player;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        
        player = FindAnyObjectByType<PlayerMovement>();
        enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None).ToList();
        
        foreach (EnemyMovement em in enemies)
        {
            if (em != null)
                em.Initialize(player.transform);
        }
    }

    private void Update()
    {
        foreach (EnemyMovement em in enemies)
        {
            if (em != null)
                em.HandleMovement();
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

}
