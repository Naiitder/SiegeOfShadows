using Unity.Mathematics;

public struct NodeData
{
    public int distance;        
    public float2 bestDir;      
    public byte walkable;       
}

public struct ProjectileData
{
    public float2 pos;
    public float radius;
    public int damage;
}

public struct HitResult
{
    public int projectileIndex;  
    public int enemyIndex;       
}