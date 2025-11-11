using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    
    [SerializeField] private List<EnemyMovement> enemies = new List<EnemyMovement>();

    public Grid grid;
    public float stopRadius = 0.25f;
    
    private TransformAccessArray taa;
    private NativeList<float> moveSpeeds;
    
    [Header("Damage on collision")]
    public float contactRadius = 0.45f;  
    public float contactCooldown = 0.5f;
    
    private NativeQueue<int> contactHits;     
    private List<float> nextAllowedHitAt;
    
    private PlayerMovement player;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        
        if (!grid) grid = FindAnyObjectByType<Grid>();
        
        if (enemies.Count == 0)
            enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None).ToList();

        taa = new TransformAccessArray(enemies.Count);
        moveSpeeds = new NativeList<float>(Allocator.Persistent);
        contactHits = new NativeQueue<int>(Allocator.Persistent);
        nextAllowedHitAt = new List<float>(enemies.Count);
        
        foreach (var em in enemies)
        {
            if (!em) continue;
            taa.Add(em.transform);
            moveSpeeds.Add(em.moveSpeed);
            nextAllowedHitAt.Add(0f); 
            em.Initialize(); 
        }
        
        player = FindAnyObjectByType<PlayerMovement>();
    }
    
    void Update()
    {
        HandleEnemiesMovement();
    }

    void HandleEnemiesMovement()
    {
        if (grid == null || grid.target == null) return;
        if (!grid.NativeReady) return; 
        
        var job = new FollowFlowJob
        {
            nodes = grid.GetNodesNative(),
            gridSize = new int2(grid.GridSizeX, grid.GridSizeY),
            gridWorldSize = grid.GridWorldSizeFloat2,
            gridCenter = grid.GridCenterFloat2,
            deltaTime = Time.deltaTime,
            stopRadius = stopRadius,
            targetPos = new float2(grid.target.position.x, grid.target.position.y),
            moveSpeeds = moveSpeeds.AsDeferredJobArray(),
            contactRadius = contactRadius,
            contactHits = contactHits.AsParallelWriter()
        };

        var handle = job.Schedule(taa);
        handle.Complete();
        
        while (contactHits.TryDequeue(out int enemyIndex))
        {
            if (enemyIndex < 0 || enemyIndex >= enemies.Count) continue;
            var enemy = enemies[enemyIndex];
            if (enemy == null || enemy.Stats == null || player.Stats == null) continue;
            
            Debug.Log("nextAllowedHitAt"+nextAllowedHitAt[enemyIndex]);
            if (Time.time < nextAllowedHitAt[enemyIndex])
            {
                Debug.Log("Time.time"+Time.time);
                continue;
            }
            
            player.Stats.TakeDamage(enemy.Stats.Damage);
            
            nextAllowedHitAt[enemyIndex] = Time.time + contactCooldown;
        }
    }
    
    public void RegisterEnemy(EnemyMovement em)
    {
        if (!em) return;
        taa.Add(em.transform);
        moveSpeeds.Add(em.moveSpeed);
        nextAllowedHitAt.Add(0f);
        enemies.Add(em);
        em.Initialize();
    }

    public void UnregisterEnemy(EnemyMovement em)
    {
        int idx = enemies.IndexOf(em);
        if (idx < 0) return;
        
        taa.RemoveAtSwapBack(idx);
        
        int last = moveSpeeds.Length - 1;
        if (idx != last) moveSpeeds[idx] = moveSpeeds[last];
        moveSpeeds.RemoveAt(last);
        
        int lastIdx = nextAllowedHitAt.Count - 1;
        if (idx != lastIdx) nextAllowedHitAt[idx] = nextAllowedHitAt[lastIdx];
        nextAllowedHitAt.RemoveAt(lastIdx);

        int lastE = enemies.Count - 1;
        enemies[idx] = enemies[lastE];
        enemies.RemoveAt(lastE);
    }

    public bool IsInList(EnemyMovement em)
    {
        return enemies.Contains(em);
    }
    
    private void OnDestroy()
    {
        if (taa.isCreated) taa.Dispose();
        if (moveSpeeds.IsCreated) moveSpeeds.Dispose();
        if (contactHits.IsCreated) contactHits.Dispose();
    }
}
