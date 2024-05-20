using Godot;

namespace dla_terrain.Procedural.Terrain.Nodes;

public partial class LandmarkNode : Node3D
{
    [Export] public Vector2I CellIndex { get; set; }
    [Export] public MeshInstance3D Center { get; set; }
    [Export] public MeshInstance3D Ground { get; set; }

    public void CenterPosition(Vector3 v)
    {
        Center.Position = v + Vector3.Up * 0.1f;
    }

    public void SetupGround(int cellSize)
    {
        Ground.Scale = new Vector3(cellSize, cellSize, cellSize);
        Ground.Position = new Vector3(cellSize / 2f, 0, cellSize / 2f);
    }

    public void GroundTexture(ImageTexture tex)
    {
        if (Ground.Mesh.SurfaceGetMaterial(0) is ShaderMaterial mat)
        {
            mat.SetShaderParameter("height_map", tex);
        }
    }
}