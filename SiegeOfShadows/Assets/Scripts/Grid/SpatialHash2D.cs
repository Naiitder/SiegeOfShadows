using System;
using System.Collections.Generic;
using UnityEngine;

public class SpatialHash2D<T>
{
    readonly float cell;
    readonly Dictionary<(int,int), List<T>> buckets = new();

    public SpatialHash2D(float cellSize) { cell = Mathf.Max(0.001f, cellSize); }

    (int,int) Key(Vector2 p) => (Mathf.FloorToInt(p.x / cell), Mathf.FloorToInt(p.y / cell));

    public void Clear() => buckets.Clear();

    public void Insert(Vector2 pos, T item)
    {
        var k = Key(pos);
        if (!buckets.TryGetValue(k, out var list)) buckets[k] = list = new List<T>(8);
        list.Add(item);
    }

    public void QueryRadius(Vector2 pos, float radius, List<T> results, Func<T, Vector2> getPos)
    {
        results.Clear();
        int minX = Mathf.FloorToInt((pos.x - radius) / cell);
        int maxX = Mathf.FloorToInt((pos.x + radius) / cell);
        int minY = Mathf.FloorToInt((pos.y - radius) / cell);
        int maxY = Mathf.FloorToInt((pos.y + radius) / cell);
        float r2 = radius * radius;

        for (int y = minY; y <= maxY; y++)
        for (int x = minX; x <= maxX; x++)
        {
            if (!buckets.TryGetValue((x,y), out var list)) continue;
            for (int i = 0; i < list.Count; i++)
            {
                var it = list[i];
                var p = getPos(it);
                if ((p - pos).sqrMagnitude <= r2) results.Add(it);
            }
        }
    }
}