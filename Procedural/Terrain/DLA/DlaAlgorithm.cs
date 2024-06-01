using System.Collections.Generic;
using System.Linq;
using dla_terrain.Utils.Godot;
using Godot;

namespace dla_terrain.Procedural.Terrain.DLA;

public class DlaAlgorithm
{
    private readonly DlaImageTexture _image;
    private readonly DlaModel _model;
    private uint[] _heights;

    public DlaAlgorithm(string seed)
    {
        var rnd = new RandomNumberGenerator();
        rnd.Seed = (ulong)GD.Hash(seed);
        _model = new DlaModel(rnd, new DlaModelConfiguration());
        _image = new DlaImageTexture(8);
    }

    public ImageTexture Run(int count)
    {
        const float k = 0.1f;
        const float m0 = 1f;
        
        _image.Reset(8);
        _model.Clear();

        _model.AddSeedParticle(Vector2.Zero);

        for (var i = 0; i < count; i++) _model.AddParticle();
        
        CalculateHeights(_model.Points);
        _image.DrawPixels(_model.Points, _heights, 4.ToVector2I(), k, m0)
            .Resize(16)
            .Blur(7);
        _model.Scale(2);
        _model.FillGaps(1);
        //
        for (var i = 0; i < count * 10; i++) _model.AddParticle();
        CalculateHeights(_model.Points);
        _image.DrawPixels(_model.Points, _heights, 8.ToVector2I(), k, m0)
            .Resize(32)
            .Blur(7);
        _model.Scale(2);
        _model.FillGaps(1);

        for (var i = 0; i < count * 20; i++) _model.AddParticle();
        CalculateHeights(_model.Points);
        _image.DrawPixels(_model.Points, _heights, 16.ToVector2I(), k, m0)
            .Resize(64)
            .Blur(7);
        _model.Scale(2);
        _model.FillGaps(1);
        
        for (var i = 0; i < count * 30; i++) _model.AddParticle();
        CalculateHeights(_model.Points);
        _image.DrawPixels(_model.Points, _heights, 32.ToVector2I(), k, m0)
            .Resize(128)
            .Blur(7);
        _model.Scale(2);
        _model.FillGaps(1);
        
        for (var i = 0; i < count * 40; i++) _model.AddParticle();
        CalculateHeights(_model.Points);
        _image.DrawPixels(_model.Points, _heights, 64.ToVector2I(), k, m0)
            .Resize(256)
            .Blur(3);

        return _image.Resize(1024).ImageTexture();
    }

    private void CalculateHeights(List<Particle> points)
    {
        _heights = new uint[points.Count];
        
        var leafIds = new Queue<int>(points.Count);
        for (var i = 0; i < points.Count; i++)
        {
            if (points[i].Neighbours.Count <= 1)
            {
                leafIds.Enqueue(i);
            }
        }
        
        while (leafIds.Any())
        {
            var id = leafIds.Dequeue();
            var point = points[id];

            uint height = 1;

            for (var n = 0; n < point.Neighbours.Count; n++)
            {
                var neighbourIndex = point.Neighbours[n];
                var neighbourHeight = _heights.Length < neighbourIndex ? 0 : _heights[point.Neighbours[n]];

                if (neighbourHeight == 0)
                {
                    leafIds.Enqueue(neighbourIndex);
                }
                else if (neighbourHeight >= height)
                {
                    height = neighbourHeight + 1;
                }
            }

            _heights[id] = height;
        }
    }
}