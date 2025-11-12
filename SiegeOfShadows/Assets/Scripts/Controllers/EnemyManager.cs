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
    private NativeList<float> contactsRadiuses;
    
    [Header("Damage on collision")]
    public float contactCooldown = 0.5f;
    
    private NativeQueue<int> contactHits;     
    private List<float> nextAllowedHitAt;
    private List<float> nextAllowedProjectileHitAt;
    
    NativeArray<float2> enemyPos;
    NativeParallelMultiHashMap<int,int> enemyHash;
    NativeArray<ProjectileData> projNative;
    NativeArray<int> hitEnemyIndex;
    
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
        contactsRadiuses = new NativeList<float>(Allocator.Persistent);
        contactHits = new NativeQueue<int>(Allocator.Persistent);
        nextAllowedHitAt = new List<float>(enemies.Count);
        nextAllowedProjectileHitAt = new List<float>(enemies.Count);
        
        foreach (var em in enemies)
        {
            if (!em) continue;
            taa.Add(em.transform);
            moveSpeeds.Add(em.moveSpeed);
            contactsRadiuses.Add(em.contactRadius);
            nextAllowedHitAt.Add(0f); 
            nextAllowedProjectileHitAt.Add(0f); 
            em.Initialize(); 
        }
        
        player = FindAnyObjectByType<PlayerMovement>();
    }
    
    void Update()
    {
        HandleJobs();
    }

    void HandleJobs()
    {
        if (grid == null || grid.target == null) return;
        if (!grid.NativeReady) return; 
        
        HandleEnemiesMovement();
        HandleHits();
    }

    void HandleEnemiesMovement()
    {
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
            contactRadiuses = contactsRadiuses.AsDeferredJobArray(),
            contactHits = contactHits.AsParallelWriter()
        };

        var handle = job.Schedule(taa);
        handle.Complete();
        
        while (contactHits.TryDequeue(out int enemyIndex))
        {
            if (enemyIndex < 0 || enemyIndex >= enemies.Count) continue;
            var enemy = enemies[enemyIndex];
            if (enemy == null || enemy.stats == null || player.stats == null) continue;
            
            if (Time.time < nextAllowedHitAt[enemyIndex]) continue;
            
            player.stats.TakeDamage(enemy.stats.Damage);
            
            nextAllowedHitAt[enemyIndex] = Time.time + contactCooldown;
        }
    }
    void HandleHits()
    {
        var projectiles = ProjectileManager.instance.Projectiles;
        
        int enemyCount = enemies?.Count ?? 0;
        int projCount  = projectiles?.Count ?? 0;
        if (enemyCount == 0 || projCount == 0) return;
        
        enemyPos = new NativeArray<float2>(enemyCount, Allocator.TempJob);
        var copyJob = new CopyEnemyPositionsJob { enemyPos = enemyPos };
        JobHandle copyHandle = copyJob.Schedule(taa);
        
        enemyHash = new NativeParallelMultiHashMap<int, int>(enemyCount * 4, Allocator.TempJob);
        var buildJob = new BuildEnemyHashJob
        {
            enemyPos = enemyPos,
            gridCenter = grid.GridCenterFloat2,
            gridWorldSize = grid.GridWorldSizeFloat2,
            gridCells = new int2(grid.GridSizeX, grid.GridSizeY),
            map = enemyHash.AsParallelWriter()
        };
        JobHandle buildHandle = buildJob.Schedule(enemyCount, 64, copyHandle);
        
        projNative = new NativeArray<ProjectileData>(projCount, Allocator.TempJob);
        hitEnemyIndex = new NativeArray<int>(projCount, Allocator.TempJob);
        for (int i = 0; i < projCount; i++)
        {
            if (projectiles != null)
            {
                var p = projectiles[i];
                if (!p) { hitEnemyIndex[i] = -1; continue; }
                var v = p.transform.position;  
                projNative[i] = new ProjectileData
                {
                    pos = new float2(v.x, v.y),
                    radius = p.Radius,    
                    damage = p.Damage         
                };
            }

            hitEnemyIndex[i] = -1;
        }
        
        var hitQueue = new NativeQueue<HitResult>(Allocator.TempJob);
        var hitJob = new ProjectileHitJob {
            projectiles = projNative,
            enemyPos    = enemyPos,
            enemyRadiuses  = contactsRadiuses.AsDeferredJobArray(),              
            enemyHash   = enemyHash,
            gridCenter  = grid.GridCenterFloat2,
            gridWorldSize = grid.GridWorldSizeFloat2,
            gridCells   = new int2(grid.GridSizeX, grid.GridSizeY),
            hits        = hitQueue.AsParallelWriter()
        };
        var hitHandle = hitJob.Schedule(projNative.Length, 64, buildHandle);
        hitHandle.Complete();

        while (hitQueue.TryDequeue(out var h))
        {
            if (projectiles != null && enemies != null)
            {
                var enemy = enemies[h.enemyIndex];
                var proj  = projectiles[h.projectileIndex];
                if (enemy != null && enemy.stats != null && proj != null)
                {
                    if (Time.time >= nextAllowedProjectileHitAt[h.enemyIndex])
                    {
                        enemy.stats.TakeDamage(proj.Damage);
                        nextAllowedProjectileHitAt[h.enemyIndex] = Time.time + proj.projectileHitCooldown;
                    }
                }
            }
        }
        
        hitQueue.Dispose();
        enemyPos.Dispose();
        enemyHash.Dispose();
        projNative.Dispose();
        hitEnemyIndex.Dispose();
    }
    
    public void RegisterEnemy(EnemyMovement em)
    {
        if (!em) return;
        taa.Add(em.transform);
        moveSpeeds.Add(em.moveSpeed);
        contactsRadiuses.Add(em.contactRadius);
        nextAllowedHitAt.Add(0f);
        nextAllowedProjectileHitAt.Add(0f);
        enemies.Add(em);
        em.Initialize();
    }

    public void UnregisterEnemy(EnemyMovement em)
    {
        int idx = enemies.IndexOf(em);
        if (idx < 0) return;
        
        taa.RemoveAtSwapBack(idx);
        
        int lastMS = moveSpeeds.Length - 1;
        if (idx != lastMS) moveSpeeds[idx] = moveSpeeds[lastMS];
        moveSpeeds.RemoveAt(lastMS);
        
        int lastC = contactsRadiuses.Length - 1;
        if (idx != lastC) contactsRadiuses[idx] = contactsRadiuses[lastC];
        contactsRadiuses.RemoveAt(lastC);
        
        int lastIdx = nextAllowedHitAt.Count - 1;
        if (idx != lastIdx) nextAllowedHitAt[idx] = nextAllowedHitAt[lastIdx];
        nextAllowedHitAt.RemoveAt(lastIdx);
        
        int lastPH = nextAllowedProjectileHitAt.Count - 1;
        if (idx != lastPH) nextAllowedProjectileHitAt[idx] = nextAllowedProjectileHitAt[lastPH];
        nextAllowedProjectileHitAt.RemoveAt(lastPH);

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
