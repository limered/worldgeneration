namespace dla_terrain.Procedural.Terrain;

public readonly record struct MapInitialization(
    int MasterSeed,
    int InitialRings,
    int MaxChunksCount,
    int R,
    int R2,
    int K)
{
}