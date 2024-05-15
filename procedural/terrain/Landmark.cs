using Godot;

namespace dla_terrain.Procedural.Terrain;

public class Landmark
{
    public Landmark(
        Vector2I cellIndex,
        Vector3 landmarkPosition,
        int cellSize)
    {
        CellIndex = cellIndex;
        Coordinate = CellIndex * cellSize;
        LandmarkPosition = landmarkPosition;
    }

    public Vector2I CellIndex { get; }
    public Vector2I Coordinate { get; }
    public Vector3 LandmarkPosition { get; private set; }
    public bool IsRendered { get; set; }
}