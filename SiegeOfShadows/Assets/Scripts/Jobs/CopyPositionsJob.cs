using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

[BurstCompile]
public struct CopyPositionsJob : IJobParallelForTransform
{
    public NativeArray<float2> positions; 

    public void Execute(int index, TransformAccess transform)
    {
        float3 position = transform.position;
        positions[index] = position.xy;
    }
}