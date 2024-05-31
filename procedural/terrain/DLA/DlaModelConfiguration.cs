﻿namespace dla_terrain.Procedural.Terrain.DLA;

public class DlaModelConfiguration
{
    public int CellSize { get; set; } = 16;
    public uint Stubbornness { get; set; } = 1;
    public float AttractionDistance { get; set; } = 1f;
    public float ParticleSpacing { get; set; } = 1f;
    public float MinMoveDistance { get; set; } = 0.1f;
    public float Stickiness { get; set; } = 1f;
}