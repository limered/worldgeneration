using dla_terrain.Hero;
using dla_terrain.SystemBase;
using Godot;

namespace dla_terrain.Procedural.Terrain.Nodes;

public partial class MapNode : Node3D
{
    [Export] private string _masterSeed;
    
    [Export] private int _pointDistanceRadius;
    [Export] private int _initialRings;
    [Export] private int _maxChunkCount;

    private SystemCollection _systems;

    private Map _map;
    
    public override void _Ready()
    {
        _systems = GetNode<SystemCollection>("/root/Systems");
        
        _map = new Map(new MapInitialization(
            GD.Hash(_masterSeed),
            _initialRings,
            _maxChunkCount,
            _pointDistanceRadius,
            _pointDistanceRadius * 2,
            30
        ));
        _map.GenerateInitial();
    }

    public override void _Process(double delta)
    {
        var heroPosition = _systems.System<HeroSystem>().HeroPosition;
        _map.Update(this, heroPosition);
    }
}