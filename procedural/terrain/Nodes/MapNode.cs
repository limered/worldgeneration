﻿using dla_terrain.Hero;
using dla_terrain.SystemBase;
using Godot;

namespace dla_terrain.Procedural.Terrain.Nodes;

public partial class MapNode : Node3D
{
    [Export] private int _initialRings;

    private MapSystem _map;
    [Export] private string _masterSeed;
    [Export] private int _maxChunkCount;

    [Export] private int _pointDistanceRadius;

    private SystemCollection _systems;

    public override void _Ready()
    {
        _systems = GetNode<SystemCollection>("/root/Systems");

        _map = _systems.System<MapSystem>()
            .Init(new MapInitialization(
                GD.Hash(_masterSeed),
                _initialRings,
                _maxChunkCount,
                _pointDistanceRadius
            )).Generate(this);
    }

    public override void _Process(double delta)
    {
        var heroPosition = _systems.System<HeroSystem>().HeroPosition;
        _map.Update(this, heroPosition);
    }
}