using dla_terrain.Utils.Random;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public record Landmark
{
    private const float FloatFactor = 1f / 255f;
    private readonly SmallXxHash _hash;
    private readonly int _hashCounter = 1;

    public Landmark(
        Vector2I cellIndex,
        Vector3 landmarkPosition,
        int cellSize,
        int masterSeed)
    {
        CellIndex = cellIndex;
        Coordinate = CellIndex * cellSize;
        IsRendered = false;
        _hash = SmallXxHash.Seed(masterSeed).Eat(Coordinate.X).Eat(Coordinate.Y);

        var x = _hash.Float01A();
        var y = _hash.Float01B();

        LandmarkPosition = new Vector3(Coordinate.X + x * cellSize, 0, Coordinate.Y + y * cellSize);
    }

    public Vector2I CellIndex { get; }
    public Vector2I Coordinate { get; }
    public Vector3 LandmarkPosition { get; private set; }
    public bool IsRendered { get; set; }
}