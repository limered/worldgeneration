using System.Collections.Generic;
using Godot;

namespace dla_terrain.Procedural.Terrain.DLA;

public struct Particle
{
    public Particle(Vector2 position) : this()
    {
        Position = position;
        Neighbours = new List<int>(16);
    }

    public Vector2 Position { get; set; }
    public List<int> Neighbours { get; init; }
}