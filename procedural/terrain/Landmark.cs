using dla_terrain.Utils.Random;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public record Landmark
{
    private const int MeshResolution = 10;
    private readonly int _cellSize;
    private readonly SmallXxHash _hash;
    private Image _image;
    private Material _mat;

    public Landmark(
        Vector2I cellIndex,
        int cellSize,
        int masterSeed)
    {
        CellIndex = cellIndex;
        CellCoordinate = new Vector3(CellIndex.X, 0, CellIndex.Y) * cellSize;
        _cellSize = cellSize;

        _hash = SmallXxHash.Seed(masterSeed).Eat(CellIndex.X).Eat(CellIndex.Y);
    }

    public MeshInstance3D Mesh { get; private set; }
    public Vector3 LandmarkPosition { get; private set; }
    public Vector2I CellIndex { get; }
    public Vector3 CellCoordinate { get; }


    public Landmark Generate()
    {
        var x = _hash.Float01A();
        var y = _hash.Float01B();
        LandmarkPosition = new Vector3(x * _cellSize, 0, y * _cellSize);

        GenerateMesh();

        return this;
    }

    private void GenerateMesh()
    {
        var planeMesh = new PlaneMesh
        {
            Size = new Vector2(_cellSize, _cellSize),
            SubdivideDepth = _cellSize * MeshResolution,
            SubdivideWidth = _cellSize * MeshResolution
        };

        var surface = new SurfaceTool();
        surface.Begin(Godot.Mesh.PrimitiveType.Triangles);
        surface.CreateFrom(planeMesh, 0);

        Mesh = new MeshInstance3D
        {
            Mesh = surface.Commit(),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.On
            // MaterialOverride = _mat
        };
    }
}