using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyToSpawn;
    public int numberOfEnemies;

    private void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Instantiate(enemyToSpawn);
        }
    }
}
