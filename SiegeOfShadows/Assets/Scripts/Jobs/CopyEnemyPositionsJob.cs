using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

[BurstCompile]
public struct CopyEnemyPositionsJob : IJobParallelForTransform
{
    public NativeArray<float2> enemyPos; 

    public void Execute(int index, TransformAccess transform)
    {
        float3 position = transform.position;
        enemyPos[index] = position.xy;
    }
}