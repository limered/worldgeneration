using Godot;

namespace dla_terrain.procedural.terrain.Sampler;

public class FastNoiseSampler : ISampler<FastNoiseLite>
{
    private FastNoiseLite _noise;

    public void Init(FastNoiseLite config)
    {
        _noise = config;
    }

    public float SampleTerrainHeight(float x, float z)
    {
        return _noise.GetNoise2D(x, z);
    }

    public void Update()
    {
        // ignore
    }
}