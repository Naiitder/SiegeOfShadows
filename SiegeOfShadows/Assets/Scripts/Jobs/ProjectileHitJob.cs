using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct ProjectileHitJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<ProjectileData> projectiles;
    [ReadOnly] public NativeArray<float2> enemyPos;
    [ReadOnly] public NativeParallelMultiHashMap<int,int> enemyHash;

    [ReadOnly] public float2 gridCenter;
    [ReadOnly] public float2 gridWorldSize;
    [ReadOnly] public int2 gridCells;
    [ReadOnly] public float enemyRadius; 

    public NativeArray<int> hitEnemyIndex;

    static int Hash(int2 c) => (c.x * 73856093) ^ (c.y * 19349663);

    public void Execute(int i)
    {
        var proj = projectiles[i];
        float2 p = proj.pos;
        float r = proj.radius + enemyRadius;
        float r2 = r * r;
        
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
                    if (math.lengthsq(ep - p) <= r2)
                    {
                        hitEnemyIndex[i] = eIdx; 
                        return;
                    }
                }
                while (enemyHash.TryGetNextValue(out eIdx, ref it));
            }
        }
        hitEnemyIndex[i] = -1;
    }
}