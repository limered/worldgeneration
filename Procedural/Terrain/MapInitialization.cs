namespace dla_terrain.Procedural.Terrain;

public readonly record struct MapInitialization(
    int MasterSeed,
    int LandmarkDistance,
    int MaxChunksCount,
    int R)
{
}