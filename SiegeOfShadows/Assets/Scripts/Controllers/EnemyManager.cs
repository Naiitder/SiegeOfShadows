using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<EnemyMovement> enemies;
    private PlayerMovement player;

    private void Awake()
    {
        player = FindAnyObjectByType<PlayerMovement>();
        
        enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None).ToList();
        foreach (EnemyMovement em in enemies)
        {
            em.Initialize(player.transform);
        }
    }

    private void Update()
    {
        foreach (EnemyMovement em in enemies)
        {
            em.HandleMovement();
        }
    }
}
