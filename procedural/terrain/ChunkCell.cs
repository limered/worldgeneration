using Godot;

namespace dla_terrain.Procedural.Terrain;

public class ChunkCell
{
    private readonly int _cellSize;
    private readonly int _masterSeed;

    private readonly RandomNumberGenerator _rnd = new();

    public ChunkCell(Vector2I cellIndex,
        Vector3 centerPoint,
        int masterSeed,
        int cellSize,
        int r,
        int r2,
        int k)
    {
        _cellSize = cellSize;
        _masterSeed = GD.Hash(masterSeed);
        CellIndex = cellIndex;
        Coordinate = CellIndex * cellSize;
        CenterPoint = centerPoint;
    }

    public Vector2I CellIndex { get; }
    public Vector2I Coordinate { get; }
    public Vector3 CenterPoint { get; private set; }
    public bool IsRendered { get; set; }
}