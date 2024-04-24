using dla_terrain.procedural.terrain.Sampler;
using Godot;

namespace dla_terrain.procedural.terrain;

public partial class TerrainGeneration : Node
{
    private MeshInstance3D _mesh;
    private int _sizeDepth = 100;
    private int _sizeWidth = 100;
    private int _meshResolution = 2;

    [Export] private FastNoiseLite _noise;

    private readonly ISampler<FastNoiseLite> _sampler = new FastNoiseSampler();

    public override void _Ready()
    {
        _sampler.Init(_noise);
        Generate();
    }

    private void Generate()
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

        for (var i = 0; i < data.GetVertexCount(); i++)
        {
            var v = data.GetVertex(i);
            v.Y = _sampler.SampleTerrainHeight(v.X, v.Z) * 50;
            data.SetVertex(i, v);
        }

        arrayPlane.ClearSurfaces();

        data.CommitToSurface(arrayPlane);
        surface.Begin(Mesh.PrimitiveType.Triangles);
        surface.CreateFrom(arrayPlane, 0);
        surface.GenerateNormals();

        _mesh = new MeshInstance3D();
        _mesh.Mesh = surface.Commit();
        _mesh.CreateTrimeshCollision();
        _mesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        _mesh.AddToGroup("NavSource");
        AddChild(_mesh);
    }
}