using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain.DLA;

public class DlaModel
{
    private readonly DlaModelConfiguration _config;

    private readonly Dictionary<Vector2I, List<int>> _index = new();
    private readonly List<uint> _joinAttempts = new();
    private readonly RandomNumberGenerator _rnd;

    private float _boundingRadius;

    public DlaModel(
        RandomNumberGenerator rnd,
        DlaModelConfiguration config)
    {
        _rnd = rnd;
        _config = config;
    }

    public List<Particle> Points { get; } = new();

    public void AddParticle()
    {
        var p = RandomStartingPosition();
        while (true)
        {
            var parentIndex = Nearest(p);
            var d = p.DistanceTo(Points[parentIndex].Position);
            if (d < _config.AttractionDistance)
            {
                if (!ShouldJoin(parentIndex))
                {
                    p = ScaleDistance(Points[parentIndex].Position, p,
                        _config.AttractionDistance + _config.MinMoveDistance);
                    continue;
                }

                p = PlaceParticle(p, parentIndex);

                Add(p, parentIndex);
                return;
            }

            var m = Mathf.Max(_config.MinMoveDistance, d - _config.AttractionDistance);
            p += MotionVector().Normalized() * m;

            if (ShouldReset(p)) p = RandomStartingPosition();
        }
    }


    public void AddSeedParticle(Vector2 v)
    {
        var p = new Particle
        {
            Position = v,
            Neighbours = new List<int>()
        };
        UpdateRadius(p.Position);
        AddToIndex(GridIndex(p.Position), Points.Count);

        _joinAttempts.Add(0);
        Points.Add(p);
    }


    public void Scale(float scale)
    {
        _index.Clear();
        for (var i = 0; i < Points.Count; i++)
        {
            var p = Points[i];
            p.Position *= scale;
            Points[i] = p;
            UpdateRadius(p.Position);
            AddToIndex(GridIndex(p.Position), i);
            _joinAttempts[i] = 0;
        }
    }

    public void FillGaps(int count)
    {
        var length = Points.Count;
        for (var i = 0; i < length; i++)
        {
            var p = Points[i];

            for (var n = 0; n < p.Neighbours.Count; n++)
            {
                var neighbour = Points[p.Neighbours[n]];

                var jitter = new Vector2(
                    _rnd.RandfRange(-1, 1),
                    _rnd.RandfRange(-1, 1)
                    );

                // ToDo: use count for more than one particle

                var newPosition = Lerp(p.Position, neighbour.Position, 0.5f);
                var newPoint = new Particle
                {
                    Position = newPosition + jitter * _config.Jitter,
                    Neighbours = new List<int>
                    {
                        p.Neighbours[n],
                        i
                    }
                };

                var pointIndex = neighbour.Neighbours.IndexOf(i);
                neighbour.Neighbours[pointIndex] = Points.Count;
                Points[p.Neighbours[n]] = neighbour;

                p.Neighbours[n] = Points.Count;

                Add(newPoint);
            }

            Points[i] = p;
        }
    }

    public void Clear()
    {
        Points.Clear();
        Points.EnsureCapacity(0);
        _index.Clear();
        _joinAttempts.Clear();
        _joinAttempts.EnsureCapacity(0);
        _boundingRadius = 0;
    }

    private void Add(Vector2 v, int parentIndex)
    {
        var p = new Particle
        {
            Position = v,
            Neighbours = new List<int> { parentIndex },
        };
        var index = Points.Count;
        UpdateRadius(v);
        AddToIndex(GridIndex(p.Position), index);

        _joinAttempts.Add(0);
        Points.Add(p);
        Points[parentIndex].Neighbours.Add(index);
    }

    private void Add(Particle p)
    {
        UpdateRadius(p.Position);
        AddToIndex(GridIndex(p.Position), Points.Count);

        _joinAttempts.Add(0);
        Points.Add(p);
    }

    private void AddToIndex(Vector2I gridIndex, int listIndex)
    {
        if (_index.TryGetValue(gridIndex, out var points)) points.Add(listIndex);
        else _index.Add(gridIndex, new List<int> { listIndex });
    }


    private void UpdateRadius(Vector2 position)
    {
        _boundingRadius = Math.Max(_boundingRadius, position.Length() + _config.AttractionDistance);
    }

    private int Nearest(Vector2 v)
    {
        var gridIndex = GridIndex(v);
        var gridCells = _index.Keys;
        var nearestCell = gridCells.ElementAt(0);
        var cellDistance = (nearestCell - gridIndex).LengthSquared();
        for (var i = 0; i < gridCells.Count; i++)
        {
            var currentCell = gridCells.ElementAt(i);
            var currentDistance = (currentCell - gridIndex).LengthSquared();
            if (currentDistance >= cellDistance) continue;

            nearestCell = currentCell;
            cellDistance = currentDistance;
        }

        var collisionPartners = _index[nearestCell];
        var nearestIndex = collisionPartners[0];
        var distance = (Points[nearestIndex].Position - v).LengthSquared();
        for (var i = 0; i < collisionPartners.Count; i++)
        {
            var currentPartnerIndex = collisionPartners[i];
            var currentDistance = (Points[currentPartnerIndex].Position - v).LengthSquared();
            if (!(currentDistance < distance)) continue;

            nearestIndex = currentPartnerIndex;
            distance = currentDistance;
        }

        return nearestIndex;
    }

    private Vector2 RandomStartingPosition()
    {
        return RandomUnitSphereMy() * _boundingRadius;
    }

    private Vector2 RandomUnitSphereMy()
    {
        var rot = _rnd.RandfRange(0, 360);
        var x = Mathf.Cos(rot);
        var y = Mathf.Sin(rot);
        return new Vector2(x, y);
    }

    private Vector2 RandomUnitSphereOther()
    {
        while (true)
        {
            var p = new Vector2(_rnd.RandfRange(-1f, 1f), _rnd.RandfRange(-1f, 1f));
            if (p.LengthSquared() < 1f) return p;
        }
    }

    private bool ShouldReset(Vector2 p)
    {
        return p.LengthSquared() > _boundingRadius * 2 * (_boundingRadius * 2);
    }

    private bool ShouldJoin(int parentIndex)
    {
        if (++_joinAttempts[parentIndex] < _config.Stubbornness) return false;
        return _rnd.Randf() <= _config.Stickiness;
    }

    private Vector2 PlaceParticle(Vector2 p, int parentIndex)
    {
        var parent = Points[parentIndex];
        var newPosition = new Vector2(
            Mathf.Lerp(parent.Position.X, p.X, _config.ParticleSpacing),
            Mathf.Lerp(parent.Position.Y, p.Y, _config.ParticleSpacing));
        return newPosition;
    }

    private Vector2 MotionVector()
    {
        return RandomUnitSphereOther();
    }

    private Vector2 ScaleDistance(Vector2 a, Vector2 b, float t)
    {
        return a + (b - a).Normalized() * t;
    }

    private Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return new Vector2(Mathf.Lerp(a.X, b.X, t), Mathf.Lerp(a.Y, b.Y, t));
    }

    private Vector2I GridIndex(Vector2 v)
    {
        var cells = v / _config.CellSize;
        return new Vector2I((int)cells.X, (int)cells.Y);
    }
}