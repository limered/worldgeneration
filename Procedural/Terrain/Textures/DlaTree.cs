using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain.Textures;

public class DlaTree
{
    private readonly RandomNumberGenerator _rnd;

    private int _baseSize;

    private DlaPoint _center;

    private int _clusterRadius;
    private float _gravity;
    private float _jitter;
    private int _newPixelsPerLayer;
    private float _stickiness;

    public DlaTree(RandomNumberGenerator rnd)
    {
        _rnd = rnd;
    }

    public List<DlaPoint> Points { get; private set; } = new();
    public Dictionary<Vector2I, int[]> Grid = new();

    private void AddToGrid(DlaPoint p)
    {
        
    }
    
    public void Reset(
        int baseSize,
        int newPixelsPerLayer,
        float gravity,
        float stickiness,
        float jitter)
    {
        _baseSize = baseSize;
        _gravity = gravity;
        _stickiness = stickiness;
        _jitter = jitter;
        _newPixelsPerLayer = newPixelsPerLayer;
        _center = new DlaPoint(new Vector2(_baseSize / 2f, _baseSize / 2f));

        Points = new List<DlaPoint> { _center };
    }
    
    public void Reset(
        int baseSize,
        int newPixelsPerLayer,
        float gravity,
        float stickiness,
        float jitter,
        Vector2 startPixel)
    {
        _baseSize = baseSize;
        _gravity = gravity;
        _stickiness = stickiness;
        _jitter = jitter;
        _newPixelsPerLayer = newPixelsPerLayer;
        _center = new DlaPoint(startPixel);

        Points = new List<DlaPoint> { _center };
    }

    public void AddNewPixelsToTree(int layerId)
    {
        var spawnCount = _newPixelsPerLayer * (layerId + 1);
        var walkers = new DlaPoint[spawnCount];
        for (var i = 0; i < walkers.Length; i++) walkers[i] = SpawnNewPointOnRadius(10);

        var border = _baseSize * (1 << layerId);
        var stuckCount = 0;
        while (stuckCount < spawnCount)
            for (var w = 0; w < walkers.Length; w++)
            {
                var walker = walkers[w];
                if (walker.Neighbours.Any()) continue;

                walker.Position += Velocity(walker);
                var nextPosition = new Vector2(
                    Math.Clamp(walker.Position.X, 1, border - 2),
                    Math.Clamp(walker.Position.Y, 1, border - 2));
                walker.Position = nextPosition;

                for (var i = 0; i < Points.Count; i++)
                {
                    var point = Points[i];
                    if ((point.Position - walker.Position).LengthSquared() > 2.25f) continue;
                    if (_rnd.Randf() > _stickiness) continue;

                    stuckCount++;

                    walker.Neighbours.Add(i);
                    point.Neighbours.Add(Points.Count);
                    Points[i] = point;
                    Points.Add(walker);
                    break;
                }

                walkers[w] = walker;
            }
    }

    private DlaPoint SpawnNewPointOnRadius(int padding)
    {
        var rot = _rnd.RandfRange(0, 360);
        var x = (_clusterRadius + padding) * Math.Cos(rot);
        var y = (_clusterRadius + padding) * Math.Sin(rot);
        return new DlaPoint(new Vector2((int)x, (int)y) + _center.Position);
    }

    private Vector2 Velocity2(DlaPoint walker)
    {
        var targetIndex = _rnd.RandiRange(0, Points.Count-1);
        return (Points[targetIndex].Position - walker.Position).Normalized() * _gravity;
    }
    
    private Vector2 Velocity(DlaPoint walker)
    {
        var rndDirection = new Vector2(
            _rnd.RandiRange(-1, 1),
            _rnd.RandiRange(-1, 1));
        var gravity = (_center.Position - walker.Position).Normalized() * _gravity;
        return rndDirection + gravity;
    }

    public void ExpandTree()
    {
        for (var i = 0; i < Points.Count; i++)
        {
            var point = Points[i];
            point.Height = 0;
            var pixelPosition = point.Position;
            pixelPosition -= _center.Position;
            pixelPosition *= 2;
            CalculateClusterRadius(pixelPosition);
            pixelPosition += _center.Position * 2;

            point.Position = pixelPosition;
            Points[i] = point;
        }

        _center.Position *= 2;
    }

    private void CalculateClusterRadius(Vector2 pixelPosition)
    {
        var radius = (_center.Position - pixelPosition).Length();
        _clusterRadius = (int)Math.Max(_clusterRadius, radius);
    }

    public void FillGaps()
    {
        var length = Points.Count;
        for (var p = 0; p < length; p++)
        {
            var point = Points[p];

            for (var n = 0; n < point.Neighbours.Count; n++)
            {
                var neighbour = Points[point.Neighbours[n]];

                var jitter = new Vector2(
                    _rnd.RandfRange(-_jitter, _jitter),
                    _rnd.RandfRange(-_jitter, _jitter));

                var direction = (neighbour.Position - point.Position) * 0.5f;
                var newPosition = point.Position + direction + jitter;
                var newPoint = new DlaPoint(newPosition);

                newPoint.Neighbours.Add(point.Neighbours[n]);
                newPoint.Neighbours.Add(p);

                var pointIndex = neighbour.Neighbours.IndexOf(p);
                neighbour.Neighbours[pointIndex] = Points.Count;
                Points[point.Neighbours[n]] = neighbour;

                point.Neighbours[n] = Points.Count;

                Points.Add(newPoint);
            }

            Points[p] = point;
        }
    }

    public int CalculateHeights()
    {
        // Collect leafs
        var leafIds = new Queue<int>();
        for (var i = 0; i < Points.Count; i++)
            if (Points[i].Neighbours.Count <= 1)
                leafIds.Enqueue(i);

        var maxHeight = 0;

        while (leafIds.Any())
        {
            var id = leafIds.Dequeue();
            var point = Points[id];

            var height = 1;
            for (var n = 0; n < point.Neighbours.Count; n++)
                if (Points[point.Neighbours[n]].Height >= height)
                    height = Points[point.Neighbours[n]].Height + 1;
                else if (Points[point.Neighbours[n]].Height == 0)
                    leafIds.Enqueue(point.Neighbours[n]);

            point.Height = height;
            Points[id] = point;

            maxHeight = Math.Max(maxHeight, height);
        }

        return maxHeight;
    }
}

public struct DlaPoint
{
    public DlaPoint(Vector2 position) : this()
    {
        Position = position;
        Neighbours = new List<int>(8);
        Height = 0;
    }

    public Vector2 Position { get; set; }
    public List<int> Neighbours { get; }
    public int Height { get; set; }
}