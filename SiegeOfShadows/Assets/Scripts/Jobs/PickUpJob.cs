using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct PickupJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> itemPos;       
    [ReadOnly] public NativeArray<float>  itemRadius;     
    [ReadOnly] public float2 playerPos;
    [ReadOnly] public float  playerPickupRadius;    

    [WriteOnly] public NativeQueue<PickUp>.ParallelWriter hits;

    public void Execute(int i)
    {
        float r  = playerPickupRadius + itemRadius[i];
        float r2 = r * r;

        if (math.lengthsq(itemPos[i] - playerPos) <= r2)
        {
            hits.Enqueue(new PickUp { itemIndex = i });
        }
    }
}