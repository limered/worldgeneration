using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public class Map
{
    private readonly List<Landmark> _landmarks;
    private readonly int _gridSize;
    private readonly MapInitialization _mapData;
    private readonly RandomNumberGenerator _rnd;

    private List<int> _activeList;

    private Vector2I _lastHeroCell;
    private PackedScene _landmarkScene;

    public Map(MapInitialization mapData)
    {
        _mapData = mapData;
        _gridSize = (int)(_mapData.R * Math.Sqrt(2));
        _landmarks = new List<Landmark>(_mapData.MaxChunksCount);
        _rnd = new RandomNumberGenerator();
        _rnd.Seed = (ulong)_mapData.MasterSeed;
        
        _landmarkScene = GD.Load<PackedScene>("res://Scenes/chunk_center.tscn");
    }

    private Vector2I ToCell(Vector3 v)
    {
        return new Vector2I((int)(v.X / _gridSize), (int)(v.Z / _gridSize));
    }

    private void GenerateLandmarks(Vector2I generateAroundIndex)
    {
        while (_activeList.Any())
        {
            var i = _rnd.RandiRange(0, _activeList.Count - 1);
            var xi = _landmarks[_activeList[i]];
            var found = false;
            for (var k = 0; k < _mapData.K; k++)
            {
                var theta = _rnd.RandfRange(0f, (float)Math.Tau);
                var dir = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta));
                dir *= _rnd.RandfRange(_mapData.R, _mapData.R2);
                var sample = dir + xi.LandmarkPosition;
                var sampleCellIndex = ToCell(sample);
                if ((sampleCellIndex - generateAroundIndex).LengthSquared() > _mapData.LandmarkDistance * _mapData.LandmarkDistance ||
                    FindCell(sampleCellIndex) != null) continue;

                var neighbours = NeighbourCenters(sampleCellIndex);
                var ok = true;
                var n = 0;
                while (ok && n < neighbours.Length)
                    ok &= (sample - neighbours[n++]).LengthSquared() >= _mapData.R * _mapData.R;

                if (!ok) continue;
                found = true;
                _activeList.Add(_landmarks.Count);
                _landmarks.Add(new Landmark(
                    sampleCellIndex,
                    sample,
                    _gridSize)
                );
            }

            if (!found) _activeList.RemoveAt(i);
        }
    }
    
    public void GenerateInitialChunks()
    {
        _activeList = new List<int>();
        _landmarks.Add(new Landmark(
            new Vector2I(0, 0),
            new Vector3(0, 0, 0),
            _gridSize));
        _activeList.Add(0);

        GenerateLandmarks(new Vector2I(0, 0));
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
        return _landmarks
            .Where(c => c != null)
            .FirstOrDefault(c => c.CellIndex == index);
    }

    public void Update(Node3D parent, Vector3 heroPosition)
    {
        var heroCell = ToCell(heroPosition);
        if (heroCell != _lastHeroCell)
        {
            var deleteList = new List<int>();
            for (var l = 0; l < _landmarks.Count; l++)
            {
                var landmark = _landmarks[l];
                var distanceSquared = (landmark.CellIndex - _lastHeroCell).LengthSquared();
                var max = _mapData.LandmarkDistance * _mapData.LandmarkDistance;
                var min = (_mapData.LandmarkDistance - 1) * (_mapData.LandmarkDistance - 1);
                if (distanceSquared > min && distanceSquared < max)
                {
                    _activeList.Add(l);
                }
                var distanceToDelete = (landmark.CellIndex - heroCell).LengthSquared();
                if (distanceToDelete >= max)
                {
                    deleteList.Add(l);
                }
            }
            
            GenerateLandmarks(heroCell);

            var ordered = deleteList.OrderByDescending(i => i);
            foreach (var i in ordered)
            {
                _landmarks.RemoveAt(i);
            }            
                        
            _lastHeroCell = heroCell;
        }

        var children = parent.GetChildren();
        for (var i = children.Count - 1; i >= 0; i--)
        {
            children[i].Free();
        }
        
        foreach (var landmark in _landmarks)
        {
            if (landmark is null or { IsRendered: true }) continue;
            // landmark.IsRendered = true;
            var sceneInstance = _landmarkScene.Instantiate<Node3D>();
            // sceneInstance.Position = new Vector3(landmark.Coordinate.X, 0, landmark.Coordinate.Y);
            sceneInstance.Position = landmark.LandmarkPosition;
            parent.AddChild(sceneInstance);
            // GD.Print(chunk.LandmarkPosition + " , " + chunk.CellIndex);
        }
    }
}