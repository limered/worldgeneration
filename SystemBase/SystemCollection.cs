using System;
using System.Collections.Generic;
using dla_terrain.Hero;
using dla_terrain.Procedural.Terrain;
using Godot;

namespace dla_terrain.SystemBase;

public partial class SystemCollection : Node
{
    private readonly Dictionary<Type, ISystem> _systems = new();

    public override void _Ready()
    {
        _systems.Add(typeof(HeroSystem), new HeroSystem());
        _systems.Add(typeof(MapSystem), new MapSystem());
    }

    public T System<T>() where T : ISystem, new()
    {
        if (_systems.TryGetValue(typeof(T), out var system)) return (T)system;

        throw new KeyNotFoundException("Can't find System with Type: " + typeof(T));
    }
}