using System;
using Godot;

namespace dla_terrain.procedural.terrain.Sampler;

public class DlaSampler : ISampler<DlaSamplerConfig>
{
    private DlaSamplerConfig _config;

    private bool[] _layer;
    private RandomNumberGenerator _randomNumberGenerator;

    public void Init(DlaSamplerConfig config)
    {
        _config = config;
        _layer = new bool[config.MaxWidth * config.MaxHeight];
        _randomNumberGenerator = new RandomNumberGenerator();
        var start = new Vector2I(
            _randomNumberGenerator.RandiRange(0, config.MaxWidth),
            _randomNumberGenerator.RandiRange(0, config.MaxHeight));
        _layer[CoordToIndex(start.X, start.Y)] = true;
    }

    public float SampleTerrainHeight(float x, float z)
    {
        return _layer[(int)x] ? 1 : 0;
    }

    public void Update()
    {
        throw new NotImplementedException();
    }

    private Vector2 IndexToCoord(int i)
    {
        var x = i % _config.MaxWidth;
        var z = i / _config.MaxWidth;
        return new Vector2(x, z);
    }

    private int CoordToIndex(int x, int z)
    {
        return x + z * _config.MaxWidth;
    }
}

public record struct DlaSamplerConfig
{
    public int MaxHeight;
    public int MaxWidth;
}