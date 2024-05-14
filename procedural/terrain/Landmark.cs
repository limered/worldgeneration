using Godot;

namespace dla_terrain.Procedural.Terrain;

public class Landmark
{
    public Landmark(Vector2I cellIndex,
        Vector3 centerPoint,
        int cellSize)
    {
        CellIndex = cellIndex;
        Coordinate = CellIndex * cellSize;
        CenterPoint = centerPoint;
    }

    public Vector2I CellIndex { get; }
    public Vector2I Coordinate { get; }
    public Vector3 CenterPoint { get; private set; }
    public bool IsRendered { get; set; }
}