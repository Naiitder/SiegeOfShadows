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
    public float stopRadius = 0.5f;
    
    private TransformAccessArray taa;
    private NativeList<float> moveSpeeds;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        
        if (!grid) grid = FindAnyObjectByType<Grid>();
        
        if (enemies.Count == 0)
            enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None).ToList();

        taa = new TransformAccessArray(enemies.Count);
        moveSpeeds = new NativeList<float>(Allocator.Persistent);
        
        foreach (var em in enemies)
        {
            if (!em) continue;
            taa.Add(em.transform);
            moveSpeeds.Add(em.moveSpeed);
            em.Initialize(); 
        }
        
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
            moveSpeeds = moveSpeeds.AsDeferredJobArray() 
        };

        var handle = job.Schedule(taa);
        handle.Complete();
    }
    
    public void RegisterEnemy(EnemyMovement em)
    {
        if (!em) return;
        taa.Add(em.transform);
        moveSpeeds.Add(em.moveSpeed);
        if (!enemies.Contains(em)) enemies.Add(em);
        em.Initialize();
    }

    public void UnregisterEnemy(EnemyMovement em)
    {
        int idx = enemies.IndexOf(em);
        if (idx < 0) return;
        
        taa.RemoveAtSwapBack(idx);
        
        int last = moveSpeeds.Length - 1;
        if (idx != last)
        {
            moveSpeeds[idx] = moveSpeeds[last];
        }
        moveSpeeds.RemoveAt(last);
        
        int lastIdx = enemies.Count - 1;
        enemies[idx] = enemies[lastIdx];
        enemies.RemoveAt(lastIdx);
    }

    public bool IsInList(EnemyMovement em)
    {
        return enemies.Contains(em);
    }
    
    private void OnDestroy()
    {
        if (taa.isCreated) taa.Dispose();
        if (moveSpeeds.IsCreated) moveSpeeds.Dispose();
    }
    
}
