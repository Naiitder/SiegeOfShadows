using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct BuildEnemyHashJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> enemyPos; 
    [ReadOnly] public float2 gridCenter;
    [ReadOnly] public float2 gridWorldSize;
    [ReadOnly] public int2 gridCells;              

    public NativeParallelMultiHashMap<int,int>.ParallelWriter map; 

    static int Hash(int2 c) => (c.x * 73856093) ^ (c.y * 19349663);

    public void Execute(int index)
    {
        float2 p = enemyPos[index];
        float2 half = gridWorldSize * 0.5f;

        float fx = math.saturate((p.x - gridCenter.x + half.x) / gridWorldSize.x);
        float fy = math.saturate((p.y - gridCenter.y + half.y) / gridWorldSize.y);

        int ix = (int)math.round((gridCells.x - 1) * fx);
        int iy = (int)math.round((gridCells.y - 1) * fy);

        int key = Hash(new int2(ix, iy));
        map.Add(key, index);
    }
}