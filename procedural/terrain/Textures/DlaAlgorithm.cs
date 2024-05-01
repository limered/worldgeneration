using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain.Textures;

public struct DlaPoint
{
    public DlaPoint(Vector2I position) : this()
    {
        Position = position;
        Neighbours = new List<int>(8);
        Height = 0;
    }

    public Vector2I Position { get; set; }
    public List<int> Neighbours { get; }

    public int Height { get; set; }
}

public class DlaAlgorithm
{
    private const int BaseSize = 8;
    private const int LayerCount = 5;
    private const int NewPixelsPerLayer = 10;
    private const float Stickiness = 0.8f;
    private const float Gravity = 0.1f;
    private const float JitterStrength = 0.01f;
    private readonly Blur2D _blur;
    private readonly Image _image;

    private readonly RandomNumberGenerator _rnd;

    private DlaPoint _center;
    private List<DlaPoint> _tree = new();
    private DlaPoint _walker;
    private int _maxHeight;

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
        _image.Resize(BaseSize, BaseSize);
        _image.Fill(Colors.Black);

        _center = new DlaPoint(new Vector2I(BaseSize / 2, BaseSize / 2));
        _tree.Add(_center);

        AddNewPixelsToTree(0);
        CalculateHeights();
        DrawTree();
        _image.Resize(BaseSize * 2, BaseSize * 2);
        _blur.BlurImage(_image, new Vector2I(3, 3));

        for (var i = 1; i < LayerCount; i++)
        {
            ExpandTree();
            FillGaps();
            AddNewPixelsToTree(i);
            CalculateHeights();
            DrawTree();
            _image.Resize(BaseSize * (2 << i), BaseSize * (2 << i));
            _blur.BlurImage(_image, new Vector2I(7, 7));
        }

        IsGenerating = false;
        return ImageTexture.CreateFromImage(_image);
    }

    private void CalculateHeights()
    {
        // Collect leafs
        var leafIds = new Queue<int>();
        for (var i = 0; i < _tree.Count; i++)
        {
            if (_tree[i].Neighbours.Count <= 1)
            {
                leafIds.Enqueue(i);
            }
        }

        _maxHeight = 0;
        
        while (leafIds.Any())
        {
            var id = leafIds.Dequeue();
            var point = _tree[id];
            
            var height = 1;
            for (var n = 0; n < point.Neighbours.Count; n++)
            {
                if (_tree[point.Neighbours[n]].Height >= height)
                {
                    height = _tree[point.Neighbours[n]].Height + 1;
                }
                else if (_tree[point.Neighbours[n]].Height == 0)
                {
                    leafIds.Enqueue(point.Neighbours[n]);
                }
            }

            point.Height = height;
            _tree[id] = point;

            _maxHeight = Math.Max(_maxHeight, height);
        }
    }

    private void FillGaps()
    {
        var length = _tree.Count;
        for (var p = 0; p < length; p++)
        {
            var point = _tree[p];

            for (var n = 0; n < point.Neighbours.Count; n++)
            {
                var neighbour = _tree[point.Neighbours[n]];

                var jitter = new Vector2(
                    _rnd.RandfRange(-JitterStrength, JitterStrength), 
                    _rnd.RandfRange(-JitterStrength, JitterStrength));
                
                var direction = (neighbour.Position - point.Position) / 2;
                var newPoint = new DlaPoint((Vector2I)(point.Position + direction + jitter));

                newPoint.Neighbours.Add(point.Neighbours[n]);
                newPoint.Neighbours.Add(p);

                var pointIndex = neighbour.Neighbours.IndexOf(p);
                neighbour.Neighbours[pointIndex] = _tree.Count;
                _tree[point.Neighbours[n]] = neighbour;

                point.Neighbours[n] = _tree.Count;

                _tree.Add(newPoint);
            }

            _tree[p] = point;
        }
    }

    private void ExpandTree()
    {
        for (var i = 0; i < _tree.Count; i++)
        {
            var point = _tree[i];
            point.Height = 0;
            var pixelPosition = point.Position;
            pixelPosition -= _center.Position;
            pixelPosition *= 2;
            pixelPosition += _center.Position * 2;

            point.Position = pixelPosition;
            _tree[i] = point;
        }

        _center.Position *= 2;
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
                // move
                _walker.Position += Velocity();

                var nextPosition = new Vector2I(
                    Math.Clamp(_walker.Position.X, 0, border - 1),
                    Math.Clamp(_walker.Position.Y, 0, border - 1));
                _walker.Position = nextPosition;

                // hit check
                for (var i = 0; i < _tree.Count; i++)
                {
                    var point = _tree[i];
                    if ((point.Position - _walker.Position).Length() > 1.5) continue;
                    if (_rnd.Randf() > Stickiness) continue;

                    stuck = true;

                    _walker.Neighbours.Add(i);
                    point.Neighbours.Add(_tree.Count);
                    _tree[i] = point;
                    _tree.Add(_walker);
                    break;
                }
            }
        }
    }

    private void SpawnNewPoint(int layerId)
    {
        var border = layerId == 0 ? BaseSize : BaseSize * (2 << (layerId - 1));
        var direction = _rnd.RandiRange(0, 4);
        _walker = direction switch
        {
            0 => new DlaPoint(new Vector2I(0, _rnd.RandiRange(0, border - 1))),
            1 => new DlaPoint(new Vector2I(_rnd.RandiRange(0, border - 1), 0)),
            2 => new DlaPoint(new Vector2I(border - 1, _rnd.RandiRange(0, border - 1))),
            _ => new DlaPoint(new Vector2I(_rnd.RandiRange(0, border - 1), border - 1))
        };
    }

    private Vector2I Velocity()
    {
        var rndDirection = new Vector2I(
            _rnd.RandiRange(-1, 1),
            _rnd.RandiRange(-1, 1));
        var gravity = ((Vector2)_center.Position - _walker.Position).Normalized() * Gravity;
        return (Vector2I)(rndDirection + gravity);
    }

    private void DrawTree()
    {
        for (var i = 0; i < _tree.Count; i++)
        {
            var point = _tree[i];
            var col = 1f - 1f / (1f + point.Height);
            _image.SetPixel(point.Position.X, point.Position.Y, new Color(col, 0, 0));
        }
    }
}