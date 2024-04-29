using dla_terrain.procedural.terrain.Sampler;
using dla_terrain.procedural.terrain.Textures;
using Godot;

namespace dla_terrain.procedural.terrain;

public partial class TerrainGeneration : Node
{
    private readonly ITerrainSampler<DlaSamplerConfig> _sampler = new DlaSampler();
    private MeshInstance3D _mesh;
    private int _meshResolution = 1;

    [Export] private FastNoiseLite _noise;
    [Export] private ShaderMaterial _mat;
    private int _sizeDepth = 50;
    private int _sizeWidth = 50;


    public override void _Ready()
    {
        _sampler.Init(new DlaSamplerConfig
        {
            Width = _sizeWidth * _meshResolution + 2,
            Height = _sizeDepth * _meshResolution + 2,
            Points = 500,
            Size = 2,
            MovementIterations = 1000,
            Noise = _noise
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

        _mat.SetShaderParameter("albedo", new DlaTexture().Create());
        _mesh.MaterialOverride = _mat;
        
        AddChild(_mesh);
    }

    private SurfaceTool CreateSurface()
    {
        var planeMesh = new PlaneMesh
        {
            Size = new Vector2(_sizeWidth, _sizeDepth),
            SubdivideDepth = _sizeDepth * _meshResolution,
            SubdivideWidth = _sizeWidth * _meshResolution,
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
            // v.Y = _sampler.SampleTerrainHeight(i, 0);
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
        // if (_sampler.Update())
        // {
        //     var surface = CreateSurface();
        //     _mesh.Mesh = surface.Commit();
        // }
    }
}