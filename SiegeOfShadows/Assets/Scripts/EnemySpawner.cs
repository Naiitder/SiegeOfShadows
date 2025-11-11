using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyToSpawn;
    public int numberOfEnemies;
    public float spawnScatterRadius = 0.5f;

    private void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector2 jitter = Random.insideUnitCircle * spawnScatterRadius;
            Vector3 pos = transform.position + new Vector3(jitter.x, jitter.y, 0);
            Instantiate(enemyToSpawn, pos, Quaternion.identity);
        }
    }
}
