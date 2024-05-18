using System.Collections.Generic;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public class LandmarkCache
{
    private readonly Dictionary<Vector2I, Landmark> _cache = new();

    public Landmark Retrieve(Vector2I coord)
    {
        return _cache[coord];
    }

    public void Cache(Vector2I coord, Landmark landmark)
    {
        _cache[coord] = landmark;
    }

    public bool Contains(Vector2I coord)
    {
        return _cache.ContainsKey(coord);
    }
}