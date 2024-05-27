using System;
using System.Collections.Generic;
using System.Linq;
using dla_terrain.Procedural.Terrain.Nodes;
using dla_terrain.SystemBase;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public class MapSystem : ISystem
{
    private Queue<int> _activeQueue;
    private LandmarkCache _cache;
    private int _gridSize;
    private List<Landmark> _landmarks;
    private PackedScene _landmarkScene;

    private Vector2I _lastHeroCell;
    private MapInitialization _mapData;

    public MapSystem Init(MapInitialization mapData)
    {
        _mapData = mapData;
        _gridSize = (int)(_mapData.R * Math.Sqrt(2));
        _landmarks = new List<Landmark>(_mapData.MaxChunksCount);
        _cache = new LandmarkCache();

        _landmarkScene = GD.Load<PackedScene>("res://Scenes/landmark.tscn");

        return this;
    }

    private Vector2I ToCell(Vector3 v)
    {
        return new Vector2I((int)(v.X / _gridSize), (int)(v.Z / _gridSize));
    }

    public MapSystem Generate(Node3D parent)
    {
        _activeQueue = new Queue<int>();
        _landmarks.Add(new Landmark(
            new Vector2I(0, 0),
            _gridSize,
            _mapData.MasterSeed).Generate());
        _activeQueue.Enqueue(0);

        var addedChildren = GenerateLandmarksCellBased(new Vector2I(0, 0));
        addedChildren.Add(new Vector2I(0, 0));

        AddNewChildren(parent, addedChildren);

        return this;
    }

    private List<Vector2I> GenerateLandmarksCellBased(Vector2I generateAroundIndex)
    {
        var generatedLandmarks = new List<Vector2I>();
        while (_activeQueue.Any())
        {
            var i = _activeQueue.Dequeue();
            var ix = _landmarks[i];
            var neighbourCoordinates = NeighbourCoordinates(ix.CellIndex);
            for (var n = 0; n < neighbourCoordinates.Length; n++)
            {
                if ((neighbourCoordinates[n] - generateAroundIndex).LengthSquared() >
                    _mapData.LandmarkDistance * _mapData.LandmarkDistance) continue;
                var neighbour = FindCell(neighbourCoordinates[n]);
                if (neighbour != null) continue;
                _activeQueue.Enqueue(_landmarks.Count);
                var landmark = _cache.Contains(neighbourCoordinates[n])
                    ? _cache.Retrieve(neighbourCoordinates[n])
                    : new Landmark(
                        neighbourCoordinates[n],
                        _gridSize,
                        _mapData.MasterSeed).Generate();

                _landmarks.Add(landmark);
                generatedLandmarks.Add(landmark.CellIndex);
            }
        }

        return generatedLandmarks;
    }

    private static Vector2I[] NeighbourCoordinates(Vector2I c)
    {
        return new[]
        {
            new Vector2I(-1, -1) + c,
            new Vector2I(-1, 0) + c,
            new Vector2I(-1, +1) + c,
            new Vector2I(0, -1) + c,
            new Vector2I(0, +1) + c,
            new Vector2I(+1, -1) + c,
            new Vector2I(+1, 0) + c,
            new Vector2I(+1, +1) + c
        };
    }

    private Landmark FindCell(Vector2I index)
    {
        return _landmarks
            .Where(c => c != null)
            .FirstOrDefault(c => c.CellIndex == index);
    }

    public void Update(Node3D parent, Vector3 heroPosition)
    {
        var deletedLandmarks = new List<Vector2I>();
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
                if (distanceSquared > min && distanceSquared < max) _activeQueue.Enqueue(l);
                var distanceToDelete = (landmark.CellIndex - heroCell).LengthSquared();
                if (distanceToDelete >= max) deleteList.Add(l);
            }

            var addedLandmarks = GenerateLandmarksCellBased(heroCell);

            var ordered = deleteList.OrderByDescending(i => i);
            foreach (var i in ordered)
            {
                deletedLandmarks.Add(_landmarks[i].CellIndex);
                _cache.Cache(_landmarks[i].CellIndex, _landmarks[i]);
                _landmarks.RemoveAt(i);
            }

            RemoveRenderedChildren(parent, deletedLandmarks);
            deletedLandmarks.Clear();
            
            AddNewChildren(parent, addedLandmarks);

            _lastHeroCell = heroCell;
        }
    }

    private void AddNewChildren(Node3D parent, List<Vector2I> addedLandmarks)
    {
        foreach (var landmarkIndex in addedLandmarks)
        {
            var landmark = FindCell(landmarkIndex);
            if (landmark is null) continue;

            var landmarkNode = _landmarkScene.Instantiate<LandmarkNode>();
            landmarkNode.Position = landmark.CellCoordinate;
            landmarkNode.CellIndex = landmark.CellIndex;
            landmarkNode.CenterPosition(landmark.LandmarkPosition);
            landmarkNode.SetupGround(_gridSize);
            landmarkNode.GroundTexture(landmark.GenerateGroundTexture(), landmark.HeightMultiplier);

            parent.AddChild(landmarkNode);
        }
    }

    private static void RemoveRenderedChildren(Node3D parent, List<Vector2I> deletedLandmarks)
    {
        var childCount = parent.GetChildCount();
        for (var d = 0; d < deletedLandmarks.Count; d++)
        for (var i = childCount - 1; i >= 0; i--)
        {
            var landmark = parent.GetChildOrNull<LandmarkNode>(i);
            if (landmark != null && landmark.CellIndex == deletedLandmarks[d])
                landmark.Free();
        }
    }
}