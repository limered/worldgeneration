using Godot;

namespace dla_terrain.Procedural.Terrain.Nodes;

public partial class MapNode : Node3D
{
    [Export] private int _chunkSize;
    [Export] private int _initialChunkCount;

    private Map _map;
    [Export] private string _masterSeed;
    [Export] private int _maxChunkCount;

    public override void _Ready()
    {
        _map = new Map(new MapInitialization(
            GD.Hash(_masterSeed),
            _initialChunkCount,
            _maxChunkCount,
            _chunkSize
        ));
        _map.GenerateInitialChunks();
    }

    public override void _Process(double delta)
    {
        _map.Update(this);
    }
}