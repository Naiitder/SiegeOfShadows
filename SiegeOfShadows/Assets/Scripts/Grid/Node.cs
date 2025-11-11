using Unity.Mathematics;
using UnityEngine;

public class Node {
    public bool isWalkable;         
    public Vector3 worldPosition;    
    public int gridX, gridY;       
    
    public int distance;       
    public Vector2 bestDirection;    
    public Node(bool _isWalkable, Vector3 _worldPos, int _gridX, int _gridY) {
        isWalkable = _isWalkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
}

public struct NodeData
{
    public int distance;        
    public float2 bestDir;      
    public byte walkable;       
}