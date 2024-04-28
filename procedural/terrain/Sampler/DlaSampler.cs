using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.procedural.terrain.Sampler;

public class DlaTree : List<Vector2I>
{
    private Dictionary<int, int[]> _neighbours = new();
    public DlaTree(int points) : base(points)
    {
    }

    public void AddNeighbour(int pointId, int newNeighbourId)
    {
        
    }
}

public class DlaSampler : ISampler<DlaSamplerConfig>
{
    private static Vector2I _center;
    private DlaSamplerConfig _config;
    private Vector2I _currentWalker;
    private RandomNumberGenerator _randomNumberGenerator;
    private DlaTree _tree;

    public void Init(DlaSamplerConfig config)
    {
        _randomNumberGenerator = new RandomNumberGenerator();
        _randomNumberGenerator.Seed = (ulong)GD.Hash("emil");

        _config = config;

        _center = new Vector2I(config.Width / 2, config.Height / 2);
        _tree = new DlaTree(config.Points)
        {
            _center
        };

        _currentWalker = new Vector2I(
            _randomNumberGenerator.RandiRange(0, config.Width),
            _randomNumberGenerator.RandiRange(0, config.Height));
    }

    public float SampleTerrainHeight(float x, float z)
    {
        for (var i = 0; i < _tree.Count; i++)
        {
            var point = _tree[i];
            if (CoordToIndex(point.X, point.Y) == (int)x) return Size(i);
        }

        return CoordToIndex(_currentWalker.X, _currentWalker.Y) == (int)x ? Size(0) : 0;
    }

    public bool Update()
    {
        if (_tree.Count >= _config.Points) return true;

        MoveCurrent();

        return true;
    }

    private Vector2I Velocity()
    {
        return new Vector2I(
            _randomNumberGenerator.RandiRange(-1, 1),
            _randomNumberGenerator.RandiRange(-1, 1));
    }

    private void MoveCurrent()
    {
        for (var i = 0; i < _config.MovementIterations; i++)
        {
            var velocity = Velocity();
            var dir = ((Vector2)_center - _currentWalker).Normalized() * (float)0.3;
            _currentWalker += velocity + (Vector2I)dir;

            _currentWalker.X = Math.Clamp(_currentWalker.X, 0, _config.Width);
            _currentWalker.Y = Math.Clamp(_currentWalker.Y, 0, _config.Height);

            var stuck = _tree
                .Select(point => (point - _currentWalker).Length())
                .Any(dist => dist <= 1.0);

            if (stuck)
            {
                _tree.Add(_currentWalker);
                SpawnNewPoint();
                break;
            }
        }
    }

    private void SpawnNewPoint()
    {
        var direction = _randomNumberGenerator.RandiRange(0, 4);
        var potentialPoint = direction switch
        {
            0 => new Vector2I(0, _randomNumberGenerator.RandiRange(0, _config.Height)),
            1 => new Vector2I(_randomNumberGenerator.RandiRange(0, _config.Width), 0),
            2 => new Vector2I(_config.Width, _randomNumberGenerator.RandiRange(0, _config.Height)),
            _ => new Vector2I(_randomNumberGenerator.RandiRange(0, _config.Width), _config.Height)
        };

        _currentWalker = potentialPoint;
    }

    private float Size(float i)
    {
        return _config.Size / _config.Points * (_config.Size - i) + _config.Size;
    }


    private Vector2I IndexToCoord(int i)
    {
        var x = i % _config.Width;
        var z = i / _config.Width;
        return new Vector2I(x, z);
    }

    private int CoordToIndex(int x, int z)
    {
        return x + z * _config.Width;
    }
}

public record struct DlaSamplerConfig
{
    public int Height;
    public int MovementIterations;
    public FastNoiseLite Noise;
    public int Points;
    public float Size;
    public int Width;
}