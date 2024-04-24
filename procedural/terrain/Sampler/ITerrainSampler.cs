namespace dla_terrain.procedural.terrain.Sampler;

public interface ITerrainSampler<in T>
{
    void Init(T config);
    float SampleTerrainHeight(float x, float z);
    bool Update();
}