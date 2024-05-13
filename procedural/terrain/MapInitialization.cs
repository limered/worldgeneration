namespace dla_terrain.Procedural.Terrain;

public readonly record struct MapInitialization(
    int MasterSeed,
    int InitialChunkCount,
    int MaxChunksCount,
    int CellSize)
{
}