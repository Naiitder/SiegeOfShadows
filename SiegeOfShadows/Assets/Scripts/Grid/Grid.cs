using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {
    public LayerMask obstacleMask;  
    public Vector2 gridWorldSize = new Vector2(200,200);  
    public float nodeRadius = 0.5f;        

    [Header("Flow Field")]
    public Transform target;          
    public float rebuildRate = 0.15f;
    
    Node[,] grid;                   
    float nodeDiameter;
    int gridSizeX, gridSizeY;
    float rebuildTimer;

    void Awake() {
        if (!target) target = FindAnyObjectByType<PlayerMovement>().transform;
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }
    
    void Update() {
        if (target == null) return;
        rebuildTimer += Time.deltaTime;
        if (rebuildTimer >= rebuildRate) {
            BuildFlowField(target.position);
            rebuildTimer = 0f;
        }
    }

    void CreateGrid() {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius, obstacleMask);

                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }
    
    public Node NodeFromWorldPoint(Vector3 worldPosition) {
        float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y - transform.position.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }
    
      public void BuildFlowField(Vector3 targetPos) {
        if (grid == null) return;
        
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                grid[x, y].distance = int.MaxValue;
                grid[x, y].bestDirection = Vector2.zero;
            }
        }

        Node targetNode = NodeFromWorldPoint(targetPos);
        if (!targetNode.isWalkable) {
            targetNode = FindClosestWalkable(targetNode);
            if (targetNode == null) return;
        }
        
        Queue<Node> q = new Queue<Node>();
        targetNode.distance = 0;
        q.Enqueue(targetNode);

        while (q.Count > 0) {
            Node current = q.Dequeue();
            var neighbors = GetNeighbors(current, allowDiagonals: true, blockCornerCutting: true);

            foreach (var n in neighbors) {
                if (!n.isWalkable) continue;

                int stepCost = (n.gridX != current.gridX && n.gridY != current.gridY) ? 14 : 10;
                int newCost = current.distance + stepCost;

                if (newCost < n.distance) {
                    n.distance = newCost;
                    q.Enqueue(n);
                }
            }
        }
        
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                Node n = grid[x, y];
                if (!n.isWalkable || n.distance == int.MaxValue) {
                    n.bestDirection = Vector2.zero;
                    continue;
                }

                int best = n.distance;
                Node bestN = n;
                var neighbors = GetNeighbors(n, allowDiagonals: true, blockCornerCutting: true);
                foreach (var nb in neighbors) {
                    if (nb.distance < best) {
                        best = nb.distance;
                        bestN = nb;
                    }
                }

                Vector2 dir = (Vector2)(bestN.worldPosition - n.worldPosition);
                n.bestDirection = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.zero;
            }
        }
    }

    Node FindClosestWalkable(Node from) {
        const int maxRing = 4;
        for (int r = 0; r <= maxRing; r++) {
            for (int dx = -r; dx <= r; dx++) {
                for (int dy = -r; dy <= r; dy++) {
                    int nx = from.gridX + dx;
                    int ny = from.gridY + dy;
                    if (nx < 0 || ny < 0 || nx >= gridSizeX || ny >= gridSizeY) continue;
                    if (grid[nx, ny].isWalkable) return grid[nx, ny];
                }
            }
        }
        return null;
    }

    List<Node> GetNeighbors(Node node, bool allowDiagonals, bool blockCornerCutting) {
        var result = new List<Node>(8);

        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                if (dx == 0 && dy == 0) continue;
                if (!allowDiagonals && dx != 0 && dy != 0) continue;

                int nx = node.gridX + dx;
                int ny = node.gridY + dy;
                if (nx < 0 || ny < 0 || nx >= gridSizeX || ny >= gridSizeY) continue;
                
                if (blockCornerCutting && dx != 0 && dy != 0) {
                    int ox = node.gridX + dx;
                    int oy = node.gridY;
                    int px = node.gridX;
                    int py = node.gridY + dy;
                    if (ox >= 0 && oy >= 0 && ox < gridSizeX && oy < gridSizeY &&
                        px >= 0 && py >= 0 && px < gridSizeX && py < gridSizeY) {
                        if (!grid[ox, oy].isWalkable || !grid[px, py].isWalkable) continue;
                    }
                }

                result.Add(grid[nx, ny]);
            }
        }

        return result;
    }

    
    void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (grid != null) {
            foreach (Node n in grid) {
                Gizmos.color = n.isWalkable ? new Color(1, 1, 1, 0.3f) : new Color(1, 0, 0, 0.5f);

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}