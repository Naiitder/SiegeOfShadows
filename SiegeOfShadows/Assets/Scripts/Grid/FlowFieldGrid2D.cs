// FlowFieldGrid2D.cs
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class FlowFieldGrid2D : MonoBehaviour
{
    [Header("Grid")]
    public Vector2 origin = Vector2.zero;
    public float cellSize = 0.5f;
    public int width = 200;
    public int height = 200;

    [Header("Obstáculos")]
    public LayerMask obstacleMask;
    public Vector2 obstacleCheckExtents = new Vector2(0.22f, 0.22f);

    [Header("Actualización")]
    public Transform player;
    public float recomputeInterval = 0.15f;
    public int maxCellsPerFrame = 10000;    
    
    private bool[] blocked;        
    private ushort[] distance;      
    private Vector2[] dir;          
    private Vector2Int lastPlayerCell = new Vector2Int(int.MinValue, int.MinValue);
    private float timer;

    int Idx(int x, int y) => y * width + x;
    bool InBounds(int x, int y) => (uint)x < width && (uint)y < height;

    public Vector2Int WorldToCell(Vector2 w) {
        var local = w - origin;
        return new Vector2Int(Mathf.FloorToInt(local.x / cellSize),
                              Mathf.FloorToInt(local.y / cellSize));
    }

    public Vector2 CellCenter(int x, int y) =>
        origin + new Vector2((x + 0.5f) * cellSize, (y + 0.5f) * cellSize);

    void Awake()
    {
        int n = width * height;
        blocked = new bool[n];
        distance = new ushort[n];
        dir = new Vector2[n];
        BakeObstacles();
        FillDistance(ushort.MaxValue);
    }

    void Update()
    {
        if (!player) return;

        timer += Time.deltaTime;
        if (timer < recomputeInterval) return;
        timer = 0f;

        var pCell = WorldToCell(player.position);
        if (!InBounds(pCell.x, pCell.y)) return;
        if (pCell == lastPlayerCell) return; 

        ComputeFlowFieldFrom(pCell);
        lastPlayerCell = pCell;
    }

    public void BakeObstacles()
    {
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            Vector2 c = CellCenter(x, y);
            blocked[Idx(x, y)] = Physics2D.OverlapBox(c, obstacleCheckExtents * 2f, 0f, obstacleMask);
        }
    }

    void FillDistance(ushort v)
    {
        for (int i = 0; i < distance.Length; i++) distance[i] = v;
    }

    void ComputeFlowFieldFrom(Vector2Int targetCell)
    {
        FillDistance(ushort.MaxValue);

        Queue<Vector2Int> q = new Queue<Vector2Int>(1024);
        int tIdx = Idx(targetCell.x, targetCell.y);
        if (blocked[tIdx]) return;

        distance[tIdx] = 0;
        q.Enqueue(targetCell);

        int visited = 0;
        
        while (q.Count > 0)
        {
            var c = q.Dequeue();
            visited++;
            if (visited > maxCellsPerFrame) break;

            ushort cd = distance[Idx(c.x, c.y)];
            foreach (var n in Neigh4(c))
            {
                if (!InBounds(n.x, n.y)) continue;
                int ni = Idx(n.x, n.y);
                if (blocked[ni]) continue;
                if (distance[ni] != ushort.MaxValue) continue;
                distance[ni] = (ushort)(cd + 1);
                q.Enqueue(n);
            }
        }
        
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int i = Idx(x, y);
            if (distance[i] == ushort.MaxValue) { dir[i] = Vector2.zero; continue; }
            
            ushort best = distance[i];
            Vector2 bestDir = Vector2.zero;
            foreach (var n in Neigh4(new Vector2Int(x, y)))
            {
                if (!InBounds(n.x, n.y)) continue;
                int ni = Idx(n.x, n.y);
                if (distance[ni] < best)
                {
                    best = distance[ni];
                    bestDir = (CellCenter(n.x, n.y) - CellCenter(x, y)).normalized;
                }
            }
            dir[i] = bestDir; 
        }
    }

    IEnumerable<Vector2Int> Neigh4(Vector2Int c)
    {
        yield return new Vector2Int(c.x + 1, c.y);
        yield return new Vector2Int(c.x - 1, c.y);
        yield return new Vector2Int(c.x, c.y + 1);
        yield return new Vector2Int(c.x, c.y - 1);
    }

    public Vector2 GetFlowDir(Vector2 worldPos)
    {
        var c = WorldToCell(worldPos);
        if (!InBounds(c.x, c.y)) return Vector2.zero;
        return dir[Idx(c.x, c.y)];
    }

    public bool IsBlocked(Vector2 worldPos)
    {
        var c = WorldToCell(worldPos);
        if (!InBounds(c.x, c.y)) return true;
        return blocked[Idx(c.x, c.y)];
    }
}