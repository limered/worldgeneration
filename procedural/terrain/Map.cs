using Godot;

namespace dla_terrain.Procedural.Terrain;

public partial class Map : Node
{
    [Export] private string _masterSeed;
    [Export] private int _initialChunkCount;
    [Export] private int _maxChunkCount;

    [Export] private int _chunkRadius;
    
    private ChunkCell[] _chunks;

    private const int K = 30;
    private int _r2;
    private int _cellSize;


    public override void _Ready()
    {
        _cellSize = (int)(_chunkRadius / Mathf.Sqrt(2));
        
        _chunks = new ChunkCell[_maxChunkCount];
        _chunks[0] = new ChunkCell(_masterSeed, _cellSize);
        _chunks[0].ReGenerate(new Vector2I(0, 0));
    }

    public override void _Process(double delta)
    {
        RenderChunks();
    }

    private void RenderChunks()
    {
        foreach (var chunk in _chunks)
        {
            if(chunk is null or { IsRendered: true }) continue;
            chunk.IsRendered = true;
            var chunkScene = GD.Load<PackedScene>("res://Scenes/chunk_center.tscn");
            var sceneInstance = chunkScene.Instantiate<Node3D>();
            AddChild(sceneInstance);
            sceneInstance.Translate(chunk.CenterPoint);
        }
    }
}