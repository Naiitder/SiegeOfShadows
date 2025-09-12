using Unity.Entities;
using Unity.Mathematics;

public struct PathfindingParams : IComponentData
{
    public int2 startPos;
    public int2 endPos;
}
