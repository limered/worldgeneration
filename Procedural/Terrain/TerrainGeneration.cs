using dla_terrain.Procedural.Terrain.DLA;
using dla_terrain.Procedural.Terrain.Poisson;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public partial class TerrainGeneration : Node
{
    private MeshInstance3D _mesh;
    private int _meshResolution = 10;
    private int _sizeDepth = 50;
    private int _sizeWidth = 50;

    [Export] private string _seed;
    [Export] private FastNoiseLite _noise;
    [Export] private ShaderMaterial _mat;

    private DlaAlgorithm _dla;
    private DiscSamplingAlgorithm _dsc;

    public override void _Ready()
    {
        _dla = new DlaAlgorithm(_seed);
        _dsc = new DiscSamplingAlgorithm(_seed);

        GenerateMesh();
        GenerateHeightMap();
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

    private void GenerateHeightMap()
    {
        if (_dsc.IsGenerating) return;
        var texture = _dla.Run(10);
        _mat.SetShaderParameter("height_map", texture);
        _mat.SetShaderParameter("height_multiplier", 1.3f);
    }

    public override void _Process(double delta)
    {
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("generate"))
        {
            GenerateHeightMap();
        }
    }
}