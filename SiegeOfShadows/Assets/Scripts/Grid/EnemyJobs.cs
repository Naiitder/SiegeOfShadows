using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct BuildHashJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> positions;
    public float cell;
    public NativeParallelMultiHashMap<int,int>.ParallelWriter hash; 

    public void Execute(int i)
    {
        float2 p = positions[i];
        int2 k = (int2)math.floor(p / cell);
        int key = (k.y << 16) ^ (k.x & 0xFFFF);
        hash.Add(key, i);
    }
}

public struct GridDataNative
{
    [ReadOnly] public NativeArray<byte> blocked;
    public int width, height;
    public float2 origin;
    public float cellSize;

    public bool InBounds(int2 c) => (uint)c.x < width && (uint)c.y < height;
    public int Idx(int2 c) => c.y * width + c.x;
    public int2 WorldToCell(float2 w) {
        float2 local = w - origin;
        return (int2)math.floor(local / cellSize);
    }
    public bool IsBlocked(float2 w) {
        var c = WorldToCell(w);
        if (!InBounds(c)) return true;
        return blocked[Idx(c)] != 0;
    }
}

[BurstCompile]
public struct DesiredDirJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> positions;
    [ReadOnly] public NativeArray<float2> flowDirsPerEnemy;
    [ReadOnly] public NativeParallelMultiHashMap<int,int> hash; // <-- aquí
    public float hashCell;
    public float separationRadius;
    public float separationWeight;

    public NativeArray<float2> desiredOut;

    public void Execute(int i)
    {
        float2 pos = positions[i];
        float2 desired = flowDirsPerEnemy[i];

        int2 baseK = (int2)math.floor(pos / hashCell);
        float r2 = separationRadius * separationRadius;
        float2 sep = 0;

        for (int oy = -1; oy <= 1; oy++)
        for (int ox = -1; ox <= 1; ox++)
        {
            int2 k = baseK + new int2(ox, oy);
            int key = (k.y << 16) ^ (k.x & 0xFFFF);

            NativeParallelMultiHashMapIterator<int> it; // <-- aquí
            int idx;
            if (hash.TryGetFirstValue(key, out idx, out it)) {
                do {
                    if (idx == i) continue;
                    float2 delta = pos - positions[idx];
                    float d2 = math.lengthsq(delta) + 1e-5f;
                    if (d2 <= r2) sep += delta / d2;
                }
                while (hash.TryGetNextValue(out idx, ref it));
            }
        }

        desired += separationWeight * sep;
        float len2 = math.lengthsq(desired);
        desiredOut[i] = (len2 > 1e-6f) ? desired / math.sqrt(len2) : float2.zero;
    }
}

[BurstCompile]
public struct MoveWithGridSlideJob : IJobParallelFor
{
    public GridDataNative grid;
    [ReadOnly] public NativeArray<float2> desiredDirs;
    [ReadOnly] public NativeArray<float> speeds;
    public float dt;
    public float maxStepFrac; // e.g., 0.45

    public NativeArray<float2> positions; // in-out

    public void Execute(int i)
    {
        float2 pos = positions[i];
        float2 total = desiredDirs[i] * speeds[i] * dt;

        float maxStep = grid.cellSize * maxStepFrac;
        int steps = math.max(1, (int)math.ceil(math.length(total) / maxStep));
        float2 step = (steps > 0) ? total / steps : float2.zero;

        for (int s = 0; s < steps; s++)
        {
            float2 tryPos = pos + step;
            if (!grid.IsBlocked(tryPos))
            {
                pos = tryPos;
            }
            else
            {
                float2 tryX = new float2(pos.x + step.x, pos.y);
                float2 tryY = new float2(pos.x, pos.y + step.y);
                bool moved = false;
                if (!grid.IsBlocked(tryX)) { pos.x = tryX.x; moved = true; }
                if (!grid.IsBlocked(tryY)) { pos.y = tryY.y; moved = true; }
                if (!moved) break;
            }
        }

        positions[i] = pos;
    }
}