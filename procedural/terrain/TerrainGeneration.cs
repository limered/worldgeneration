using dla_terrain.procedural.terrain.Sampler;
using Godot;

namespace dla_terrain.procedural.terrain;

public partial class TerrainGeneration : Node
{
    private readonly ISampler<DlaSamplerConfig> _sampler = new DlaSampler();
    private MeshInstance3D _mesh;
    private int _meshResolution = 2;

    [Export] private FastNoiseLite _noise;
    private int _sizeDepth = 50;
    private int _sizeWidth = 50;

    public override void _Ready()
    {
        _sampler.Init(new DlaSamplerConfig
        {
            MaxWidth = _sizeWidth * _meshResolution + 2,
            MaxHeight = _sizeDepth * _meshResolution + 2,
            MaxPoints = 2000,
            MaxSize = 5
        });
        Generate();
    }

    private void Generate()
    {
        var surface = CreateSurface();

        _mesh = new MeshInstance3D();
        _mesh.Mesh = surface.Commit();
        // _mesh.CreateTrimeshCollision();
        _mesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.On;
        // _mesh.AddToGroup("NavSource");
        AddChild(_mesh);
    }

    private SurfaceTool CreateSurface()
    {
        var planeMesh = new PlaneMesh
        {
            Size = new Vector2(_sizeWidth, _sizeDepth),
            SubdivideDepth = _sizeDepth * _meshResolution,
            SubdivideWidth = _sizeWidth * _meshResolution
        };

        var surface = new SurfaceTool();
        var data = new MeshDataTool();
        surface.CreateFrom(planeMesh, 0);

        var arrayPlane = surface.Commit();
        data.CreateFromSurface(arrayPlane, 0);

        var vertexCount = data.GetVertexCount();
        for (var i = 0; i < vertexCount; i++)
        {
            var v = data.GetVertex(i);
            v.Y = _sampler.SampleTerrainHeight(i, 0);
            data.SetVertex(i, v);
        }

        arrayPlane.ClearSurfaces();

        data.CommitToSurface(arrayPlane);
        surface.Begin(Mesh.PrimitiveType.Triangles);
        surface.CreateFrom(arrayPlane, 0);
        surface.GenerateNormals();
        return surface;
    }

    public override void _Process(double delta)
    {
        if (_sampler.Update())
        {
            var surface = CreateSurface();
            _mesh.Mesh = surface.Commit();
        }
    }
}