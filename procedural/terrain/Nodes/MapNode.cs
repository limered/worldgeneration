using Godot;

namespace dla_terrain.Procedural.Terrain.Nodes;

public partial class MapNode : Node3D
{
    [Export] private string _masterSeed;
    
    [Export] private int _pointDistanceRadius;
    [Export] private int _initialRings;
    [Export] private int _maxChunkCount;

    private Map _map;
    public override void _Ready()
    {
        _map = new Map(new MapInitialization(
            GD.Hash(_masterSeed),
            _initialRings,
            _maxChunkCount,
            _pointDistanceRadius,
            _pointDistanceRadius * 2,
            30
        ));
        _map.GenerateInitialChunks();
    }

    public override void _Process(double delta)
    {
        _map.Update(this);
    }
}