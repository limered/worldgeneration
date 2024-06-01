using dla_terrain.Utils.Godot;
using Godot;

namespace dla_terrain.Procedural.Terrain.DLA;

public class DlaAlgorithm
{
    private readonly DlaImageTexture _image;
    private readonly DlaModel _model;
    private readonly RandomNumberGenerator _rnd;

    public DlaAlgorithm(string seed)
    {
        _rnd = new RandomNumberGenerator();
        _rnd.Seed = (ulong)GD.Hash(seed);
        _model = new DlaModel(_rnd, new DlaModelConfiguration());
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
        var heights = _model.CalculateHeights();
        _image.DrawPixels(_model.Points, heights, 4.ToVector2I(), k, m0)
            .Resize(16)
            .Blur(7);
        _model.Scale(2);
        _model.FillGaps(1);
        //
        for (var i = 0; i < count * 10; i++) _model.AddParticle();
        heights = _model.CalculateHeights();
        _image.DrawPixels(_model.Points, heights, 8.ToVector2I(), k, m0)
            .Resize(32)
            .Blur(7);
        _model.Scale(2);
        _model.FillGaps(1);

        for (var i = 0; i < count * 20; i++) _model.AddParticle();
        heights = _model.CalculateHeights();
        _image.DrawPixels(_model.Points, heights, 16.ToVector2I(), k, m0)
            .Resize(64)
            .Blur(7);
        _model.Scale(2);
        _model.FillGaps(1);
        
        for (var i = 0; i < count * 30; i++) _model.AddParticle();
        heights = _model.CalculateHeights();
        _image.DrawPixels(_model.Points, heights, 32.ToVector2I(), k, m0)
            .Resize(128)
            .Blur(7);
        _model.Scale(2);
        _model.FillGaps(1);
        
        for (var i = 0; i < count * 40; i++) _model.AddParticle();
        heights = _model.CalculateHeights();
        _image.DrawPixels(_model.Points, heights, 64.ToVector2I(), k, m0)
            .Resize(256)
            .Blur(3);

        return _image.Resize(1024).ImageTexture();
    }
}