using dla_terrain.Hero;
using dla_terrain.SystemBase;
using Godot;

namespace dla_terrain.Procedural.Terrain.Nodes;

public partial class MapNode : Node3D
{
    [Export] private string _masterSeed;
    [Export] private int _initialRings;
    [Export] private int _maxChunkCount;
    [Export] private int _pointDistanceRadius;

    [Export] private FastNoiseLite _baseHeight;

    private MapSystem _map;
    private SystemCollection _systems;

    public override void _Ready()
    {
        _systems = GetNode<SystemCollection>("/root/Systems");
        if (_baseHeight != null) _baseHeight.Frequency = 0.1f / _pointDistanceRadius;
        
        _map = _systems.System<MapSystem>()
            .Init(new MapInitialization(
                GD.Hash(_masterSeed),
                _initialRings,
                _maxChunkCount,
                _pointDistanceRadius,
                _baseHeight
            )).Generate(this);
    }

    public override void _Process(double delta)
    {
        var heroPosition = _systems.System<HeroSystem>().HeroPosition;
        _map.Update(this, heroPosition);
    }
}