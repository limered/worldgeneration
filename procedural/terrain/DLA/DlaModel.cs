using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain.DLA;

public class DlaModel
{
    private const int CellSize = 16;
    private const uint Stubbornness = 1;
    
    private const float AttractionDistance = 1f;
    private const float ParticleSpacing = 1f;
    
    private const float MinMoveDistance = 5f;
    private const float Stickiness = 0.7f;

    private readonly Dictionary<Vector2I, List<int>> _index = new();
    private readonly List<uint> _joinAttempts = new();

    private readonly RandomNumberGenerator _rnd;

    private float _boundingRadius;

    public DlaModel(RandomNumberGenerator rnd)
    {
        _rnd = rnd;
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
                    p = Lerp(Points[parentIndex].Position, p, _config.AttractionDistance + _config.MinMoveDistance);
                    continue;
                }

                p = PlaceParticle(p, parentIndex);

                Add(p, parentIndex);
                return;
            }

            var m = Mathf.Max(MinMoveDistance, d - AttractionDistance);
            p += MotionVector().Normalized() * m;

            if (ShouldReset(p)) p = RandomStartingPosition();
        }
    }

    public void Add(Vector2 v)
    {
        var p = new Particle
        {
            Position = v,
            Height = 0,
            Neighbours = new List<int>()
        };
        var index = Points.Count;
        var gridIndex = GridIndex(p.Position);
        _boundingRadius = Math.Max(_boundingRadius, v.Length() + AttractionDistance);

        if (_index.TryGetValue(gridIndex, out var points)) points.Add(index);
        else _index.Add(gridIndex, new List<int>{ index });

        _joinAttempts.Add(0);
        Points.Add(p);
    }

    private void Add(Vector2 v, int parentIndex)
    {
        var p = new Particle
        {
            Position = v,
            Neighbours = new List<int>{ parentIndex },
            Height = 0
        };
        var index = Points.Count;
        var gridIndex = GridIndex(p.Position);
        _boundingRadius = Math.Max(_boundingRadius, v.Length() + AttractionDistance);

        if (_index.TryGetValue(gridIndex, out var points)) points.Add(index);
        else _index.Add(gridIndex, new List<int>{index});

        _joinAttempts.Add(0);
        Points.Add(p);
        Points[parentIndex].Neighbours.Add(index);
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
        if (++_joinAttempts[parentIndex] < Stubbornness) return false;
        return _rnd.Randf() <= Stickiness;
    }

    private Vector2 PlaceParticle(Vector2 p, int parentIndex)
    {
        var parent = Points[parentIndex];
        var newPosition = new Vector2(
            Mathf.Lerp(parent.Position.X, p.X, ParticleSpacing),
            Mathf.Lerp(parent.Position.Y, p.Y, ParticleSpacing));
        return newPosition;
    }

    private Vector2 MotionVector()
    {
        return RandomUnitSphereOther();
    }

    private Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return a + (b - a).Normalized() * t;
        // return new Vector2(
        //     Mathf.Lerp(a.X, b.X, t),
        //     Mathf.Lerp(a.Y, b.Y, t)
        // );
    }

    private static Vector2I GridIndex(Vector2 v)
    {
        var cells = v / CellSize;
        return new Vector2I((int)cells.X, (int)cells.Y);
    }
}