namespace dla_terrain.Procedural.Terrain.Sampler;

public interface ITerrainSampler<in T>
{
    void Init(T config);
    float SampleTerrainHeight(float x, float z);
    bool Update();
}