using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public class Map
{
    private readonly List<Landmark> _chunks;
    private readonly int _gridSize;
    private readonly MapInitialization _mapData;
    private readonly RandomNumberGenerator _rnd;

    private List<int> _activeList;

    private Vector2I _lastHeroCell;

    public Map(MapInitialization mapData)
    {
        _mapData = mapData;
        _gridSize = (int)(_mapData.R * Math.Sqrt(2));
        _chunks = new List<Landmark>(_mapData.MaxChunksCount);
        _rnd = new RandomNumberGenerator();
        _rnd.Seed = (ulong)_mapData.MasterSeed;
    }

    private Vector2I ToCell(Vector3 v)
    {
        return new Vector2I((int)(v.X / _gridSize), (int)(v.Z / _gridSize));
    }

    public void GenerateInitialChunks()
    {
        _activeList = new List<int>();
        _chunks.Add(new Landmark(
            new Vector2I(0, 0),
            new Vector3(0, 0, 0),
            _gridSize));
        _activeList.Add(0);

        while (_activeList.Any())
        {
            var i = _rnd.RandiRange(0, _activeList.Count - 1);
            var xi = _chunks[_activeList[i]];
            var found = false;
            for (var k = 0; k < _mapData.K; k++)
            {
                var theta = _rnd.RandfRange(0f, (float)Math.Tau);
                var dir = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta));
                dir *= _rnd.RandfRange(_mapData.R, _mapData.R2);
                var sample = dir + xi.LandmarkPosition;
                var sampleCellIndex = ToCell(sample);
                if (Math.Abs(sampleCellIndex.X) > _mapData.InitialRings ||
                    Math.Abs(sampleCellIndex.Y) > _mapData.InitialRings ||
                    FindCell(sampleCellIndex) != null) continue;

                var neighbours = NeighbourCenters(sampleCellIndex);
                var ok = true;
                var n = 0;
                while (ok && n < neighbours.Length)
                    ok &= (sample - neighbours[n++]).LengthSquared() >= _mapData.R * _mapData.R;

                if (!ok) continue;
                found = true;
                _activeList.Add(_chunks.Count);
                _chunks.Add(new Landmark(
                    sampleCellIndex,
                    sample,
                    _gridSize)
                );
            }

            if (!found) _activeList.RemoveAt(i);
        }
    }

    private Vector3[] NeighbourCenters(Vector2I c)
    {
        return new[]
            {
                FindCell(new Vector2I(-1, -1) + c),
                FindCell(new Vector2I(-1, 0) + c),
                FindCell(new Vector2I(-1, +1) + c),
                FindCell(new Vector2I(0, -1) + c),
                FindCell(new Vector2I(0, +1) + c),
                FindCell(new Vector2I(+1, -1) + c),
                FindCell(new Vector2I(+1, 0) + c),
                FindCell(new Vector2I(+1, +1) + c)
            }
            .Where(chunk => chunk != null)
            .Select(chunk => chunk.LandmarkPosition)
            .ToArray();
    }

    private Landmark FindCell(Vector2I index)
    {
        return _chunks
            .Where(c => c != null)
            .FirstOrDefault(c => c.CellIndex == index);
    }

    public void Update(Node3D parent, Vector3 heroPosition)
    {
        var heroCell = ToCell(heroPosition);
        if (heroCell != _lastHeroCell)
        {
            // TODO calculate new landmarks and delete old ones
            GD.Print(heroCell);
            _lastHeroCell = heroCell;
        }

        foreach (var chunk in _chunks)
        {
            if (chunk is null or { IsRendered: true }) continue;
            chunk.IsRendered = true;
            var chunkScene = GD.Load<PackedScene>("res://Scenes/chunk_center.tscn");
            var sceneInstance = chunkScene.Instantiate<Node3D>();
            parent.AddChild(sceneInstance);
            // sceneInstance.Position = new Vector3(chunk.Coordinate.X, 0, chunk.Coordinate.Y);
            sceneInstance.Position = chunk.LandmarkPosition;
            // GD.Print(chunk.LandmarkPosition + " , " + chunk.CellIndex);
        }
    }
}