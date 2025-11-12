using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ProjectileHitJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<ProjectileData> projectiles;
    [ReadOnly] public NativeArray<float2> enemyPos;
    [ReadOnly] public NativeArray<float> enemyRadiuses; 
    [ReadOnly] public NativeParallelMultiHashMap<int,int> enemyHash;

    [ReadOnly] public float2 gridCenter;
    [ReadOnly] public float2 gridWorldSize;
    [ReadOnly] public int2 gridCells;

    [WriteOnly] public NativeQueue<HitResult>.ParallelWriter hits;

    static int Hash(int2 c) => (c.x * 73856093) ^ (c.y * 19349663);

    public void Execute(int i)
    {
        var proj = projectiles[i];
        float2 p = proj.pos;
        
        float2 half = gridWorldSize * 0.5f;
        float fx = math.saturate((p.x - gridCenter.x + half.x) / gridWorldSize.x);
        float fy = math.saturate((p.y - gridCenter.y + half.y) / gridWorldSize.y);
        int cx = (int)math.round((gridCells.x - 1) * fx);
        int cy = (int)math.round((gridCells.y - 1) * fy);
        
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            int2 c = new int2(cx + dx, cy + dy);
            int key = Hash(c);

            if (enemyHash.TryGetFirstValue(key, out int eIdx, out var it))
            {
                do
                {
                    float2 ep = enemyPos[eIdx];
                    float   er = enemyRadiuses.IsCreated ? enemyRadiuses[eIdx] : 0f;
                    
                    float r  = proj.radius + er;
                    float r2 = r * r;
                    
                    if (math.lengthsq(ep - p) <= r2)
                    {
                        hits.Enqueue(new HitResult { projectileIndex = i, enemyIndex = eIdx });
                    }
                }
                while (enemyHash.TryGetNextValue(out eIdx, ref it));
            }
        }
    }
}