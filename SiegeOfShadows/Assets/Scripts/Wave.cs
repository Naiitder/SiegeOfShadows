using UnityEngine;

[System.Serializable]
public class Wave 
{
    public string name;
    public float startTime;        
    public GameObject enemyPrefab;
    public float spawnRate = 1f;
}
