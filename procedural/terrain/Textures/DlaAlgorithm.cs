using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace dla_terrain.Procedural.Terrain.Textures;

public class DlaAlgorithm
{
    private const int BaseSize = 8;
    private const int LayerCount = 5;
    private const int NewPixelsPerLayer = 7;
    private const float Stickiness = 0.5f;
    private const float Gravity = 0.05f;
    private const float JitterStrength = 0.01f;
    private readonly Image _image;

    private readonly DlaTree _tree;
    private Stopwatch _stopwatch;

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
        _stopwatch = new Stopwatch();
        IsGenerating = true;

        _image.Resize(BaseSize, BaseSize);
        _image.Fill(Colors.Black);
        
        _stopwatch.Start();
        _tree.Reset(BaseSize, NewPixelsPerLayer, Gravity, Stickiness, JitterStrength);
        _tree.AddNewPixelsToTree(0);
        _tree.CalculateHeights();
        DrawTree(_tree.Points);
        _image.Resize(BaseSize * 2, BaseSize * 2);
        Blur2D.BlurImage(_image, new Vector2I(3, 3));
        GD.Print("Layer 0 " + _stopwatch.ElapsedMilliseconds);
        _stopwatch.Restart();

        for (var i = 1; i < LayerCount; i++)
        {
            _tree.ExpandTree();
            
            GD.Print("Layer " + i + " expand " + _stopwatch.ElapsedMilliseconds);
            _stopwatch.Restart();
            
            _tree.FillGaps();
            
            GD.Print("Layer " + i + " fill " + _stopwatch.ElapsedMilliseconds);
            _stopwatch.Restart();
            
            _tree.AddNewPixelsToTree(i);
            
            GD.Print("Layer " + i + " new pixel " + _stopwatch.ElapsedMilliseconds);
            _stopwatch.Restart();
            
            _tree.CalculateHeights();
            
            GD.Print("Layer " + i + " heights " + _stopwatch.ElapsedMilliseconds);
            _stopwatch.Restart();
            
            DrawTree(_tree.Points);
            
            GD.Print("Layer " + i + " draw " + _stopwatch.ElapsedMilliseconds);
            _stopwatch.Restart();
            
            _image.Resize(BaseSize * (2 << i), BaseSize * (2 << i));
            
            GD.Print("Layer " + i + " resize " + _stopwatch.ElapsedMilliseconds);
            _stopwatch.Restart();
            
            Blur2D.BlurImage(_image, new Vector2I(7, 7));
            
            GD.Print("Layer " + i + " blur " + _stopwatch.ElapsedMilliseconds);
            _stopwatch.Restart();
        }
        _stopwatch.Stop();
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