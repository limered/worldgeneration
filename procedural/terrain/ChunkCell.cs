using System.Linq;
using dla_terrain.Utils.Random;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public class ChunkCell
{
    private readonly int _cellSize;
    private readonly int _masterSeed;

    private readonly RandomNumberGenerator _rnd = new();

    public ChunkCell(Vector2I coordinate,
        int masterSeed,
        int cellSize,
        int r,
        int r2,
        int k)
    {
        _cellSize = cellSize;
        _masterSeed = GD.Hash(masterSeed);
        Coordinate = coordinate;
    }

    public Vector2I Coordinate { get; }
    public Vector3 CenterPoint { get; private set; }
    public bool IsRendered { get; set; }

    public ChunkCell ReGenerate(Vector3[] neighbourCenters, int r)
    {
        _rnd.Seed = SmallXxHash.Seed(_masterSeed)
            .Eat(Coordinate.X).Eat(Coordinate.Y);

        var found = false;
        while (!found)
        {
            CenterPoint = new Vector3(
                _rnd.RandfRange(-_cellSize / 2f, _cellSize / 2f) + Coordinate.X, 0,
                _rnd.RandfRange(-_cellSize / 2f, _cellSize / 2f) + Coordinate.Y);
            if (neighbourCenters.All(c => (c - CenterPoint).Length() >= r/2f))
            {
                found = true;
            }
        }

        GD.Print(CenterPoint);
        return this;
    }
}