using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.procedural.terrain.Sampler;

public class DlaSampler : ISampler<DlaSamplerConfig>
{
    private static Vector2I _center;
    private DlaSamplerConfig _config;
    private Vector2I _currentWalker;
    private RandomNumberGenerator _randomNumberGenerator;
    private List<Vector2I> _tree;

    public void Init(DlaSamplerConfig config)
    {
        _randomNumberGenerator = new RandomNumberGenerator();

        _config = config;

        _center = new Vector2I(config.MaxWidth / 2, config.MaxHeight / 2);
        _tree = new List<Vector2I>(config.MaxPoints)
        {
            _center
        };

        _currentWalker = new Vector2I(
            _randomNumberGenerator.RandiRange(0, config.MaxWidth),
            _randomNumberGenerator.RandiRange(0, config.MaxHeight));
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
        if (_tree.Count >= _config.MaxPoints) return true;

        _currentWalker.X += _randomNumberGenerator.RandiRange(-1, 1);
        _currentWalker.Y += _randomNumberGenerator.RandiRange(-1, 1);

        var dir = ((Vector2)_center - _currentWalker) * (float)0.3;
        _currentWalker += (Vector2I)dir;

        _currentWalker.X = Math.Clamp(_currentWalker.X, 0, _config.MaxWidth);
        _currentWalker.Y = Math.Clamp(_currentWalker.Y, 0, _config.MaxHeight);

        var stuck = _tree
            .Select(point => (point - _currentWalker).Length())
            .Any(dist => dist <= 1.0);

        if (!stuck) return false;

        _tree.Add(_currentWalker);
        SpawnNewPoint();
        
        return false;
    }

    private void SpawnNewPoint()
    {
        var potentialPoint = new Vector2I(
            _randomNumberGenerator.RandiRange(0, _config.MaxWidth),
            _randomNumberGenerator.RandiRange(0, _config.MaxHeight));

        while (_tree.Contains(potentialPoint))
            potentialPoint = new Vector2I(
                _randomNumberGenerator.RandiRange(0, _config.MaxWidth),
                _randomNumberGenerator.RandiRange(0, _config.MaxHeight));

        _currentWalker = potentialPoint;
    }

    private float Size(float i)
    {
        return (_config.MaxSize / _config.MaxPoints * (_config.MaxSize - i)) + _config.MaxSize;
    }


    private Vector2I IndexToCoord(int i)
    {
        var x = i % _config.MaxWidth;
        var z = i / _config.MaxWidth;
        return new Vector2I(x, z);
    }

    private int CoordToIndex(int x, int z)
    {
        return x + z * _config.MaxWidth;
    }
}

public record struct DlaSamplerConfig
{
    public int MaxHeight;
    public int MaxPoints;
    public float MaxSize;
    public int MaxWidth;
}