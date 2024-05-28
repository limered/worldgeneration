﻿using dla_terrain.Utils.Godot;
using Godot;

namespace dla_terrain.Procedural.Terrain.DLA;

public class DlaAlgorithm
{
    private readonly RandomNumberGenerator _rnd;
    private DlaModel _model;

    public DlaAlgorithm()
    {
        _rnd = new RandomNumberGenerator();
        _rnd.Seed = (ulong)GD.Hash("emil");
        _model = new DlaModel(_rnd);
    }

    public ImageTexture Run(int count)
    {
        const int size = 256;
        var image = Image.Create(size, size, false, Image.Format.Rf);

        _model = new DlaModel(_rnd);
        _model.Add(Vector2.Zero);
        for (var i = 0; i < count; i++) _model.AddParticle();

        foreach (var point in _model.Points)
        {
            var coord = (Vector2I)point.Position + (size / 2).ToVector2I();
            if (coord.X < 0 || coord.X > size-1 || coord.Y < 0 || coord.Y > size-1) continue;
            image.SetPixelv(coord, Colors.White);
        }

        return ImageTexture.CreateFromImage(image);
    }
}