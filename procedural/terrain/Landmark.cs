using dla_terrain.Utils.Random;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public record Landmark
{
    private readonly int _cellSize;
    private readonly SmallXxHash _hash;

    public Landmark(
        Vector2I cellIndex,
        int cellSize,
        int masterSeed)
    {
        CellIndex = cellIndex;
        Coordinate = CellIndex * cellSize;
        _cellSize = cellSize;

        _hash = SmallXxHash.Seed(masterSeed).Eat(Coordinate.X).Eat(Coordinate.Y);
    }


    public Vector3 LandmarkPosition { get; private set; }
    public Vector2I CellIndex { get; }
    public Vector2I Coordinate { get; }


    public Landmark Generate()
    {
        var x = _hash.Float01A();
        var y = _hash.Float01B();

        LandmarkPosition = new Vector3(Coordinate.X + x * _cellSize, 0, Coordinate.Y + y * _cellSize);

        return this;
    }
}