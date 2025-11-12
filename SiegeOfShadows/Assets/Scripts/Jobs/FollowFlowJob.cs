using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

[BurstCompile(FloatPrecision.Low, FloatMode.Fast, CompileSynchronously = true)]
public struct FollowFlowJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<NodeData> nodes;
    [ReadOnly] public int2 gridSize;
    [ReadOnly] public float2 gridWorldSize; 
    [ReadOnly] public float2 gridCenter;
    [ReadOnly] public float deltaTime;
    [ReadOnly] public float stopRadius;
    [ReadOnly] public float2 targetPos;
    [ReadOnly] public NativeArray<float> moveSpeeds; 
    
    [ReadOnly] public NativeArray<float> contactRadiuses;
    [WriteOnly] public NativeQueue<int>.ParallelWriter contactHits;

    public void Execute(int index, TransformAccess transform)
    {
        float3 p3 = transform.position;
        float2 p = p3.xy;

        float2 toTarget = targetPos - p;
        float dist = math.length(toTarget);
        
        float contactRadius = contactRadiuses[index];
        
        if (dist <= contactRadius)
        {
            contactHits.Enqueue(index);
        }

        if (dist <= stopRadius)
            return; 
        
        float2 half = gridWorldSize * 0.5f;

        float fx = math.saturate((p.x - gridCenter.x + half.x) / gridWorldSize.x);
        float fy = math.saturate((p.y - gridCenter.y + half.y) / gridWorldSize.y);

        int ix = (int)math.round((gridSize.x - 1) * fx);
        int iy = (int)math.round((gridSize.y - 1) * fy);

        int flat = ix + iy * gridSize.x;
        float2 dir = float2.zero;

        if (flat >= 0 && flat < nodes.Length)
        {
            dir = nodes[flat].bestDir;
        }

        if (math.lengthsq(dir) < 1e-8f)
        {
            dir = dist > 1e-5f ? toTarget / dist : float2.zero;
        }
        
        float speed = moveSpeeds[index];
        
        float2 step = dir * speed * deltaTime;
        float2 newP = p + step;
        transform.position = new float3(newP, p3.z);
        

    }
}