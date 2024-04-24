using Godot;

namespace dla_terrain.procedural.terrain.Sampler;

public class FastNoiseSampler : ISampler<FastNoiseLite>
{
    private FastNoiseLite _noise;

    public void Init(FastNoiseLite noise)
    {
        _noise = noise;
    }

    public float SampleTerrainHeight(float x, float z)
    {
        return _noise.GetNoise2D(x, z);
    }
}