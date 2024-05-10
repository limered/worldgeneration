using dla_terrain.Utils.Random;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public class ChunkCell
{
    private readonly int _cellSize;
    public Vector2I Coordinate { get; private set; }
    public Vector3 CenterPoint { get; private set; }
    public bool IsRendered { get; set; }
    private readonly int _masterSeed;

    private readonly RandomNumberGenerator _rnd = new();

    public ChunkCell(string masterSeed, int cellSize)
    {
        _cellSize = cellSize;
        _masterSeed = GD.Hash(masterSeed);
    }

    public void ReGenerate(Vector2I coordinate)
    {
        Coordinate = coordinate;
        _rnd.Seed = SmallXxHash.Seed(_masterSeed)
            .Eat(Coordinate.X).Eat(Coordinate.Y);

        CenterPoint = new Vector3(
            _rnd.RandfRange(-_cellSize / 2f, _cellSize / 2f),
            0,
            _rnd.RandfRange(-_cellSize / 2f, _cellSize / 2f));
        
        GD.Print(CenterPoint);
    }
}