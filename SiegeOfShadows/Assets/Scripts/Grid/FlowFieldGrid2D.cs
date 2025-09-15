#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

using System.Collections.Generic;

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
    [Min(0f)] public float obstacleEpsilon = 0.02f;

    [Header("Actualización")]
    public Transform player;
    public float recomputeInterval = 0.15f;
    public int maxCellsPerFrame = 10000;    
    
    private bool[] blocked;        
    private ushort[] distance;      
    private Vector2[] dir;          
    private Vector2Int lastPlayerCell = new Vector2Int(int.MinValue, int.MinValue);
    private float timer;
    
    static readonly int[] DX   = {  1, -1,  0,  0,  1, -1,  1, -1 };
    static readonly int[] DY   = {  0,  0,  1, -1,  1,  1, -1, -1 };
    static readonly ushort[] COST = { 10, 10, 10, 10, 14, 14, 14, 14 };
    const int ORTH_COUNT = 4;  
    const int ALL_COUNT  = 8;  
    
    [Header("Gizmos")]
    public bool drawGrid = true;
    public bool drawBlocked = true;
    public bool drawFlow = false;
    [Range(1, 10)] public int arrowStride = 2; // solo para flechas del flow
    public Color gridColor = new Color(0f, 1f, 1f, 0.9f);
    public Color blockedColor = new Color(1f, 0f, 0f, 0.25f);
    public Color flowColor = new Color(1f, 1f, 0f, 0.9f);
    [Range(0.1f, 1.0f)] public float flowArrowScale = 0.45f;

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
        var size = new Vector2(
            Mathf.Max(0.001f, cellSize - 2f * obstacleEpsilon),
            Mathf.Max(0.001f, cellSize - 2f * obstacleEpsilon)
        );

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            Vector2 c = CellCenter(x, y);
            blocked[Idx(x, y)] = Physics2D.OverlapBox(c, size, 0f, obstacleMask);
        }
    }

    void FillDistance(ushort v)
    {
        for (int i = 0; i < distance.Length; i++) distance[i] = v;
    }
    void ComputeFlowFieldFrom(Vector2Int targetCell)
{
    FillDistance(ushort.MaxValue);

    int tIdx = Idx(targetCell.x, targetCell.y);
    if (!InBounds(targetCell.x, targetCell.y)) return;
    if (blocked[tIdx]) return;

    MinHeap heap = new MinHeap();
    distance[tIdx] = 0;
    heap.Push(tIdx, 0);
    
    while (heap.Count > 0)
    {
        heap.Pop(out int curIdx, out int curDist);
        if (curDist > distance[curIdx]) continue;

        int cx = curIdx % width;
        int cy = curIdx / width;

        int nCount = ALL_COUNT; 
        for (int k = 0; k < nCount; k++)
        {
            int nx = cx + DX[k];
            int ny = cy + DY[k];
            if (!InBounds(nx, ny)) continue;

            int nIdx = Idx(nx, ny);
            if (blocked[nIdx]) continue;

            bool isDiag = k >= ORTH_COUNT;
            if (isDiag)
            {
                int ix1 = Idx(nx, cy);
                int ix2 = Idx(cx, ny);
                if (blocked[ix1] || blocked[ix2]) continue;
            }

            int newDist = curDist + COST[k];
            if (newDist < distance[nIdx])
            {
                distance[nIdx] = (ushort)Mathf.Min(newDist, ushort.MaxValue);
                heap.Push(nIdx, newDist);
            }
        }
    }
    
    for (int y = 0; y < height; y++)
    for (int x = 0; x < width; x++)
    {
        int i = Idx(x, y);
        if (distance[i] == ushort.MaxValue) { dir[i] = Vector2.zero; continue; }

        ushort best = distance[i];
        Vector2 bestDir = Vector2.zero;

        int nCount = ALL_COUNT; 
        for (int k = 0; k < nCount; k++)
        {
            int nx = x + DX[k];
            int ny = y + DY[k];
            if (!InBounds(nx, ny)) continue;

            int ni = Idx(nx, ny);
            if (distance[ni] < best)
            {
                best = distance[ni];
                bestDir = (CellCenter(nx, ny) - CellCenter(x, y)).normalized;
            }
        }
        dir[i] = bestDir;
    }
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
    
void OnDrawGizmosSelected()
{
    if (cellSize <= 0f || width <= 0 || height <= 0) return;

    Vector3 start = new Vector3(origin.x, origin.y, 0f);
    Vector3 size = new Vector3(width * cellSize, height * cellSize, 0f);
    
    Gizmos.color = gridColor;
    Gizmos.DrawWireCube(start + new Vector3(size.x * 0.5f, size.y * 0.5f, 0f), size);


#if UNITY_EDITOR
    if (drawGrid)
    {
        Handles.color = gridColor;

        for (int x = 0; x <= width; x++)
        {
            Vector3 a = start + new Vector3(x * cellSize, 0f, 0f);
            Vector3 b = a + new Vector3(0f, height * cellSize, 0f);
            Handles.DrawLine(a, b);
        }

        for (int y = 0; y <= height; y++)
        {
            Vector3 a = start + new Vector3(0f, y * cellSize, 0f);
            Vector3 b = a + new Vector3(width * cellSize, 0f, 0f);
            Handles.DrawLine(a, b);
        }
    }
#endif
    
    bool hasBlocked = (blocked != null && blocked.Length == width * height);
    if (drawBlocked && hasBlocked)
    {
        Gizmos.color = blockedColor;
        Vector3 cellSizeV = new Vector3(cellSize, cellSize, 0f);

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            if (!blocked[Idx(x, y)]) continue;
            Gizmos.DrawCube((Vector3)CellCenter(x, y), cellSizeV);
        }
    }
    
    bool hasDir = (dir != null && dir.Length == width * height);
    if (drawFlow && hasDir)
    {
        Gizmos.color = flowColor;
        int s = Mathf.Max(1, arrowStride);

        for (int y = 0; y < height; y += s)
        for (int x = 0; x < width; x += s)
        {
            Vector2 v = dir[Idx(x, y)];
            if (v.sqrMagnitude < 1e-4f) continue;

            Vector3 center = (Vector3)CellCenter(x, y);
            Vector3 a = center;
            Vector3 b = center + (Vector3)(v.normalized * cellSize * flowArrowScale);

            Gizmos.DrawLine(a, b);
            Vector3 n = (b - a).normalized;
            Vector3 left = Quaternion.Euler(0, 0, 150f) * n * (cellSize * 0.2f);
            Vector3 right = Quaternion.Euler(0, 0, -150f) * n * (cellSize * 0.2f);
            Gizmos.DrawLine(b, b - left);
            Gizmos.DrawLine(b, b - right);
        }
    }
}
}

struct HeapNode { public int idx; public int dist; }

class MinHeap
{
    readonly List<HeapNode> h = new List<HeapNode>(1024);
    public int Count => h.Count;

    public void Push(int idx, int dist)
    {
        h.Add(new HeapNode { idx = idx, dist = dist });
        SiftUp(h.Count - 1);
    }

    public void Pop(out int idx, out int dist)
    {
        var root = h[0];
        idx = root.idx; dist = root.dist;

        int last = h.Count - 1;
        h[0] = h[last];
        h.RemoveAt(last);
        if (h.Count > 0) SiftDown(0);
    }

    void SiftUp(int i)
    {
        while (i > 0)
        {
            int p = (i - 1) >> 1;
            if (h[p].dist <= h[i].dist) break;
            (h[p], h[i]) = (h[i], h[p]);
            i = p;
        }
    }

    void SiftDown(int i)
    {
        int n = h.Count;
        while (true)
        {
            int l = (i << 1) + 1;
            int r = l + 1;
            int s = i;

            if (l < n && h[l].dist < h[s].dist) s = l;
            if (r < n && h[r].dist < h[s].dist) s = r;
            if (s == i) break;

            (h[s], h[i]) = (h[i], h[s]);
            i = s;
        }
    }
}