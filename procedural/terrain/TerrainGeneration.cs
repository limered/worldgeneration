using dla_terrain.Procedural.Terrain.Textures;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public partial class TerrainGeneration : Node
{
    private MeshInstance3D _mesh;
    private int _meshResolution = 4;
    private int _sizeDepth = 50;
    private int _sizeWidth = 50;

    [Export] private string _seed;
    [Export] private FastNoiseLite _noise;
    [Export] private ShaderMaterial _mat;

    private DlaAlgorithm _dla;

    public override void _Ready()
    {
        _dla = new DlaAlgorithm(_seed);

        GenerateMesh();
        Generate();
    }

    private void GenerateMesh()
    {
        var planeMesh = new PlaneMesh
        {
            Size = new Vector2(_sizeWidth, _sizeDepth),
            SubdivideDepth = _sizeDepth * _meshResolution,
            SubdivideWidth = _sizeWidth * _meshResolution
        };

        var surface = new SurfaceTool();
        surface.Begin(Mesh.PrimitiveType.Triangles);
        surface.CreateFrom(planeMesh, 0);

        _mesh = new MeshInstance3D
        {
            Mesh = surface.Commit(),
            CastShadow = GeometryInstance3D.ShadowCastingSetting.On,
            MaterialOverride = _mat
        };

        AddChild(_mesh);
    }

    private void Generate()
    {
        var texture = _dla.Create();
        _mat.SetShaderParameter("albedo", texture);
    }

    public override void _Process(double delta)
    {
        
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("generate"))
        {
            Generate();
        }
    }
}