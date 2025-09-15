using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    
    [SerializeField] private List<EnemyMovement> enemies = new List<EnemyMovement>();
    private PlayerMovement player;
    [SerializeField] private FlowFieldGrid2D flow;
    
    [Header("Separación / Hash")]
    [SerializeField] private float separationRadius = 0.5f;
    [SerializeField] private float separationWeight = 0.6f;
    [SerializeField] private float hashCellSize = 1.0f;

    private NativeArray<float2> positions;
    private NativeArray<float2> flowDirsPerEnemy;
    private NativeArray<float2> desiredDirs;
    private NativeArray<float> speeds;
    private NativeParallelMultiHashMap<int,int> hash;
    private bool nativeDirty = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        
        if (!player) player = FindAnyObjectByType<PlayerMovement>();
        if (enemies.Count == 0)
            enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None).ToList();

        foreach (var em in enemies) if (em) em.Initialize(player.transform, flow);

        AllocateNative(enemies.Count);
    }
    
    void FixedUpdate()
    {
        int n = enemies.Count;
        if (n == 0 || flow == null) return;
        
        if (!positions.IsCreated || positions.Length != n || nativeDirty)
        {
            AllocateNative(n);
            nativeDirty = false;
        }
        
        for (int i = 0; i < n; i++)
        {
            var em = enemies[i];
            if (!em) continue;

            Vector2 p = em.transform.position;
            positions[i] = new float2(p.x, p.y);

            Vector2 flowDir = flow.GetFlowDir(p);
            flowDirsPerEnemy[i] = new float2(flowDir.x, flowDir.y);

            speeds[i] = em.moveSpeed; 
        }
        
        hash.Clear(); 
        var buildHash = new BuildHashJob
        {
            positions = positions,
            cell = hashCellSize,
            hash = hash.AsParallelWriter()
        }.Schedule(n, 64);
        
        var desiredJob = new DesiredDirJob
        {
            positions = positions,
            flowDirsPerEnemy = flowDirsPerEnemy,
            hash = hash,
            hashCell = hashCellSize,
            separationRadius = separationRadius,
            separationWeight = separationWeight,
            desiredOut = desiredDirs
        }.Schedule(n, 64, buildHash);
        
        var gd = flow.GetGridDataNative(); 
        var moveJob = new MoveWithGridSlideJob
        {
            grid = new GridDataNative
            {
                blocked = gd.blocked,
                width = gd.width,
                height = gd.height,
                origin = gd.origin,
                cellSize = gd.cellSize
            },
            desiredDirs = desiredDirs,
            speeds = speeds,
            dt = Mathf.Min(Time.fixedDeltaTime, 1f / 30f),
            maxStepFrac = 0.45f,
            positions = positions
        }.Schedule(n, 64, desiredJob);
        
        moveJob.Complete();

        for (int i = 0; i < n; i++)
        {
            var em = enemies[i];
            if (!em) continue;

            float2 p = positions[i];
            em.ApplyJobPosition(new Vector2(p.x, p.y), Time.fixedDeltaTime);
        }
    }
    
    
    void OnDestroy()
    {
        DisposeNative();
    }

    void AllocateNative(int n)
    {
        DisposeNative();

        if (n <= 0) n = 1;

        positions       = new NativeArray<float2>(n, Allocator.Persistent);
        flowDirsPerEnemy= new NativeArray<float2>(n, Allocator.Persistent);
        desiredDirs     = new NativeArray<float2>(n, Allocator.Persistent);
        speeds          = new NativeArray<float>(n, Allocator.Persistent);
        hash            = new NativeParallelMultiHashMap<int,int>(n * 2, Allocator.Persistent);
    }

    void DisposeNative()
    {
        if (positions.IsCreated) positions.Dispose();
        if (flowDirsPerEnemy.IsCreated) flowDirsPerEnemy.Dispose();
        if (desiredDirs.IsCreated) desiredDirs.Dispose();
        if (speeds.IsCreated) speeds.Dispose();
        if (hash.IsCreated) hash.Dispose();
    }

    public void RegisterEnemy(EnemyMovement em)
    {
        enemies.Add(em);
        em.Initialize(player.transform, flow);
        nativeDirty = true;
    }

    public void UnregisterEnemy(EnemyMovement em)
    {
        enemies.Remove(em);
        nativeDirty = true;
    }

    public bool IsInList(EnemyMovement em)
    {
        return enemies.Contains(em);
    }
    
    public EnemyMovement GetClosestEnemy(Vector2 origin, float maxDistance = Mathf.Infinity)
    {
        EnemyMovement closest = null;
        float best = maxDistance;
        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (!e) continue;
            float d = Vector2.Distance(origin, e.transform.position);
            if (d < best) { best = d; closest = e; }
        }
        return closest;
    }

}
