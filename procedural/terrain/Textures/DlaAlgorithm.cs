using System.Collections.Generic;
using Godot;

namespace dla_terrain.Procedural.Terrain.Textures;

public class DlaAlgorithm
{
    private const int BaseSize = 8;
    private const int LayerCount = 5;
    private const int NewPixelsPerLayer = 10;
    private const float Stickiness = 0.8f;
    private const float Gravity = 0.2f;
    private const float JitterStrength = 0.01f;

    private readonly DlaTree _tree;
    private readonly Image _image;

    public DlaAlgorithm(string seed)
    {
        var rnd = new RandomNumberGenerator();
        if (!string.IsNullOrEmpty(seed)) rnd.Seed = (ulong)GD.Hash(seed);

        _image = Image.Create(BaseSize, BaseSize, false, Image.Format.Rgbaf);
        _tree = new DlaTree(rnd);
    }

    public bool IsGenerating { get; private set; }

    public ImageTexture Create()
    {
        IsGenerating = true;
        
        _image.Resize(BaseSize, BaseSize);
        _image.Fill(Colors.Black);

        _tree.Reset(BaseSize, NewPixelsPerLayer, Gravity, Stickiness, JitterStrength);
        _tree.AddNewPixelsToTree(0);
        _tree.CalculateHeights();
        DrawTree(_tree.Points);
        _image.Resize(BaseSize * 2, BaseSize * 2);
        Blur2D.BlurImage(_image, new Vector2I(3, 3));

        for (var i = 1; i < LayerCount; i++)
        {
            _tree.ExpandTree();
            _tree.FillGaps();
            _tree.AddNewPixelsToTree(i);
            _tree.CalculateHeights();
            DrawTree(_tree.Points);
            _image.Resize(BaseSize * (2 << i), BaseSize * (2 << i));
            Blur2D.BlurImage(_image, new Vector2I(7, 7));
        }

        IsGenerating = false;
        return ImageTexture.CreateFromImage(_image);
    }

    private void DrawTree(IReadOnlyList<DlaPoint> points)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var col = 1f - 1f / (1f + point.Height);
            _image.SetPixel(point.Position.X, point.Position.Y, new Color(col, 0, 0));
        }
    }
}