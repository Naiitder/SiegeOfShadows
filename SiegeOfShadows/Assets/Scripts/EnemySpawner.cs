using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public float spawnDistance = 10f;
    public int maxEnemiesOnScreen = 100;
    
    public Wave[] waves;
    private float timer;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>().transform;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        if (currentWave < waves.Length - 1 && timer >= waves[currentWave + 1].startTime)
        {
            currentWave++;
        }
        
        if (enemiesAlive < maxEnemiesOnScreen)
        {
            StartCoroutine(SpawnRoutine());
        }
    }
    
    IEnumerator SpawnRoutine()
    {
        var wave = waves[currentWave];
        yield return new WaitForSeconds(wave.spawnRate);
        
        Vector2 spawnPos = GetSpawnPosition();
        GameObject enemy = Instantiate(wave.enemyPrefab, spawnPos, Quaternion.identity);
        
        enemiesAlive++;
        
        enemy.GetComponent<EnemyController>().stats.OnDeath += () => enemiesAlive--;
    }

    private Vector2 GetSpawnPosition()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        return (Vector2)player.position + dir * spawnDistance;
    }
}
