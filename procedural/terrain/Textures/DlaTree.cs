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
    private float _gravity;
    private float _jitter;
    private int _newPixelsPerLayer;
    private float _stickiness;
    private DlaPoint _walker;

    public DlaTree(RandomNumberGenerator rnd)
    {
        _rnd = rnd;
    }

    public List<DlaPoint> Points { get; private set; } = new();

    public void Reset(int baseSize, int newPixelsPerLayer, float gravity, float stickiness, float jitter)
    {
        _baseSize = baseSize;
        _gravity = gravity;
        _stickiness = stickiness;
        _jitter = jitter;
        _newPixelsPerLayer = newPixelsPerLayer;
        _center = new DlaPoint(new Vector2I(_baseSize / 2, _baseSize / 2));
        
        Points = new List<DlaPoint> { _center };
    }

    public void AddNewPixelsToTree(int layerId)
    {
        var border = layerId == 0 ? _baseSize : _baseSize * (2 << (layerId - 1));

        for (var walkerIndex = 0; walkerIndex < _newPixelsPerLayer * (layerId + 1); walkerIndex++)
        {
            SpawnNewPoint(layerId);

            var stuck = false;
            while (!stuck)
            {
                // move
                _walker.Position += Velocity();

                var nextPosition = new Vector2I(
                    Math.Clamp(_walker.Position.X, 0, border - 1),
                    Math.Clamp(_walker.Position.Y, 0, border - 1));
                _walker.Position = nextPosition;

                // hit check
                for (var i = 0; i < Points.Count; i++)
                {
                    var point = Points[i];
                    if ((point.Position - _walker.Position).Length() > 1.5) continue;
                    if (_rnd.Randf() > _stickiness) continue;

                    stuck = true;

                    _walker.Neighbours.Add(i);
                    point.Neighbours.Add(Points.Count);
                    Points[i] = point;
                    Points.Add(_walker);
                    break;
                }
            }
        }
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
            pixelPosition += _center.Position * 2;

            point.Position = pixelPosition;
            Points[i] = point;
        }

        _center.Position *= 2;
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

                var direction = (neighbour.Position - point.Position) / 2;
                var newPoint = new DlaPoint((Vector2I)(point.Position + direction + jitter));

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
                else if (Points[point.Neighbours[n]].Height == 0) leafIds.Enqueue(point.Neighbours[n]);

            point.Height = height;
            Points[id] = point;

            maxHeight = Math.Max(maxHeight, height);
        }

        return maxHeight;
    }

    private Vector2I Velocity()
    {
        var rndDirection = new Vector2I(
            _rnd.RandiRange(-1, 1),
            _rnd.RandiRange(-1, 1));
        var gravity = ((Vector2)_center.Position - _walker.Position).Normalized() * _gravity;
        return (Vector2I)(rndDirection + gravity);
    }

    private void SpawnNewPoint(int layerId)
    {
        var border = layerId == 0 ? _baseSize : _baseSize * (2 << (layerId - 1));
        var direction = _rnd.RandiRange(0, 4);
        _walker = direction switch
        {
            0 => new DlaPoint(new Vector2I(0, _rnd.RandiRange(0, border - 1))),
            1 => new DlaPoint(new Vector2I(_rnd.RandiRange(0, border - 1), 0)),
            2 => new DlaPoint(new Vector2I(border - 1, _rnd.RandiRange(0, border - 1))),
            _ => new DlaPoint(new Vector2I(_rnd.RandiRange(0, border - 1), border - 1))
        };
    }
}

public struct DlaPoint
{
    public DlaPoint(Vector2I position) : this()
    {
        Position = position;
        Neighbours = new List<int>(8);
        Height = 0;
    }

    public Vector2I Position { get; set; }
    public List<int> Neighbours { get; }
    public int Height { get; set; }
}