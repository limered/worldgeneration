using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain.Textures;

public struct DlaPoint
{
    public Vector2I Position { get; set; }
    public int GenerationLayer { get; set; }
}

public class DlaAlgorithm
{
    private const int BaseSize = 8;
    private const int LayerCount = 4;
    private const int NewPixelsPerLayer = 7;
    private readonly Blur2D _blur;
    private readonly Image _image;

    private readonly RandomNumberGenerator _rnd;

    private DlaPoint _center;
    private List<DlaPoint> _tree = new();
    private DlaPoint _walker;

    public DlaAlgorithm(string seed)
    {
        _rnd = new RandomNumberGenerator();
        if (!string.IsNullOrEmpty(seed)) _rnd.Seed = (ulong)GD.Hash(seed);

        _blur = new Blur2D();
        _image = Image.Create(BaseSize, BaseSize, false, Image.Format.Rgbaf);
    }

    public bool IsGenerating { get; private set; }

    public ImageTexture Create()
    {
        IsGenerating = true;

        _tree = new List<DlaPoint>();
        _image.Fill(Colors.Black);

        _center = new DlaPoint
        {
            Position = new Vector2I(BaseSize / 2, BaseSize / 2),
            GenerationLayer = 0
        };
        _tree.Add(_center);

        AddNewPixelsToTree(0);
        DrawTree();
        _image.Resize(BaseSize * 2, BaseSize * 2);
        _blur.BlurImage(_image, new Vector2I(3, 3));

        for (var i = 1; i < LayerCount; i++)
        {
            ExpandTree();
            FillGaps();
            AddNewPixelsToTree(i);
            DrawTree();
            _image.Resize(BaseSize * (2 << i), BaseSize * (2 << i));
            _blur.BlurImage(_image, new Vector2I(5, 5));    
        }
        
        IsGenerating = false;
        return ImageTexture.CreateFromImage(_image);
    }

    private void FillGaps()
    {
        var tempTree = new List<DlaPoint>();
        for (var i = 0; i < _tree.Count; i++)
        {
            var point = _tree[i];
            if (i == 0) continue;
            var j = i - 1;
            while (j >= 0 && _tree[j].Position.X <= point.Position.X && _tree[j].Position.X >= point.Position.X - 2)
            {
                // Check for top connection
                if (_tree[j].Position.X == point.Position.X &&
                    _tree[j].Position.Y == point.Position.Y - 2 &&
                    _tree[j].GenerationLayer == point.GenerationLayer)
                    tempTree.Add(point with
                    {
                        Position = new Vector2I(point.Position.X, point.Position.Y - 1)
                    });

                // Check for left connection
                if (_tree[j].Position.X == point.Position.X - 2 &&
                    _tree[j].Position.Y == point.Position.Y &&
                    _tree[j].GenerationLayer == point.GenerationLayer)
                    tempTree.Add(point with
                    {
                        Position = new Vector2I(point.Position.X - 1, point.Position.Y)
                    });

                j--;
            }
        }

        _tree.AddRange(tempTree);
    }

    private void ExpandTree()
    {
        var tempTree = new DlaPoint[_tree.Count];
        for (var i = 0; i < _tree.Count; i++)
        {
            var pixelPosition = _tree[i].Position;
            // translate
            pixelPosition -= _center.Position;
            // scale
            pixelPosition *= 2;
            // translateBack
            pixelPosition += _center.Position * 2;

            tempTree[i].Position = pixelPosition;
        }

        _center.Position *= 2;
        _tree = tempTree.OrderBy(v => v.Position.X).ThenBy(v => v.Position.Y).ToList();
    }

    private void AddNewPixelsToTree(int layerId)
    {
        var border = layerId == 0 ? BaseSize : BaseSize * (2 << (layerId - 1));

        for (var walkerIndex = 0; walkerIndex < NewPixelsPerLayer * (layerId + 1); walkerIndex++)
        {
            SpawnNewPoint(layerId);

            var stuck = false;
            while (!stuck)
            {
                var vel = Velocity();
                var dir = ((Vector2)_center.Position - _walker.Position).Normalized() * (float)0.3;
                _walker.Position += vel + (Vector2I)dir;

                var nextPosition = new Vector2I(Math.Clamp(_walker.Position.X, 0, border - 1),
                    Math.Clamp(_walker.Position.Y, 0, border - 1));
                _walker.Position = nextPosition;

                stuck = _tree
                    .Select(point => (point.Position - _walker.Position).Length())
                    .Any(dist => dist <= 1.0);

                if (!stuck) continue;

                _tree.Add(_walker);
            }
        }
    }

    private void SpawnNewPoint(int layerId)
    {
        var border = layerId == 0 ? BaseSize : BaseSize * (2 << (layerId - 1));
        var direction = _rnd.RandiRange(0, 4);
        _walker = direction switch
        {
            0 => new DlaPoint { Position = new Vector2I(0, _rnd.RandiRange(0, border - 1)), GenerationLayer = layerId },
            1 => new DlaPoint { Position = new Vector2I(_rnd.RandiRange(0, border - 1), 0), GenerationLayer = layerId },
            2 => new DlaPoint
                { Position = new Vector2I(border - 1, _rnd.RandiRange(0, border - 1)), GenerationLayer = layerId },
            _ => new DlaPoint
                { Position = new Vector2I(_rnd.RandiRange(0, border - 1), border - 1), GenerationLayer = layerId }
        };
    }

    private Vector2I Velocity()
    {
        return new Vector2I(
            _rnd.RandiRange(-1, 1),
            _rnd.RandiRange(-1, 1));
    }

    private void DrawTree()
    {
        for (var i = 0; i < _tree.Count; i++)
        {
            var point = _tree[i];
            _image.SetPixel(point.Position.X, point.Position.Y, Colors.White);
        }
    }
}