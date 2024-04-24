namespace dla_terrain.procedural.terrain.Sampler;

public interface ISampler<in T>
{
    void Init(T generator);
    float SampleTerrainHeight(float x, float z);
}