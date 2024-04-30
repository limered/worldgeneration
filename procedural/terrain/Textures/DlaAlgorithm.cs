using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain.Textures;

public class DlaAlgorithm
{
    private const int StartSize = 8;
    private const int LayerCount = 5;
    private const int NewPixelsPerLayer = 7;

    private readonly Image[] _layers = new Image[LayerCount];
    private readonly RandomNumberGenerator _rnd;
    private List<Vector2I> _tree = new();

    private Vector2I _center;
    private Vector2I _walker;

    public DlaAlgorithm(string seed)
    {
        _rnd = new RandomNumberGenerator();
        if(!string.IsNullOrEmpty(seed))
        {
            _rnd.Seed = (ulong)GD.Hash(seed);
        }
    }

    public ImageTexture Create()
    {
        InitLayers();
        
        _center = new Vector2I(StartSize / 2, StartSize/2);
        _tree.Add(_center);
        
        AddNewPixelsToTree(0);

        int i;
        for (i = 1; i < 5; i++)
        {
            ExpandTree();
            FillGaps();
            // RotateTree();
            AddNewPixelsToTree(i);
        }
        
        DrawTree(i-1);

        return ImageTexture.CreateFromImage(_layers[i-1]);
    }

    private void RotateTree()
    {
        const float rotationParam = Mathf.Pi / 18;
        var theta = _rnd.RandfRange(-rotationParam, rotationParam);
        var transform = new Transform2D(theta, Vector2.Zero);
        var tempTree = new Vector2I[_tree.Count];
        for (var i = 0; i < _tree.Count; i++)
        {
            var pixel = _tree[i];
            pixel -= _center;
            pixel = (Vector2I)(pixel * transform);
            pixel += _center;

            tempTree[i] = pixel;
        }

        _tree = tempTree.ToList();
    }

    private void FillGaps()
    {
        var tempTree = new List<Vector2I>();
        for (var i = 0; i < _tree.Count; i++)
        {
            var point = _tree[i];
            if(i == 0) continue;
            var j = i-1;
            while (j >= 0 && _tree[j].X <= point.X && _tree[j].X >= point.X - 2)
            {
                // Check for top connection
                if (_tree[j].X == point.X && _tree[j].Y == point.Y - 2)
                {
                    tempTree.Add(new Vector2I(point.X, point.Y-1));
                }
                // Check for left connection
                if (_tree[j].X == point.X - 2 && _tree[j].Y == point.Y)
                {
                    tempTree.Add(new Vector2I(point.X-1, point.Y));
                    
                }

                j--;
            }
        }

        _tree.AddRange(tempTree);
    }

    private void ExpandTree()
    {
        var tempTree = new Vector2I[_tree.Count];
        for (var i = 0; i < _tree.Count; i++)
        {
            var pixel = _tree[i];
            // translate
            pixel -= _center;
            // scale
            pixel *= 2;
            // translateBack
            pixel += _center * 2;

            tempTree[i] = pixel;
        }

        _center *= 2;
        _tree = tempTree.OrderBy(v => v.X).ThenBy(v => v.Y).ToList();
    }

    private void InitLayers()
    {
        for (var i = 0; i < LayerCount; i++)
        {
            var size = i == 0 ? StartSize : StartSize * (2 << (i - 1));
            _layers[i] = Image.Create(size, size, false, Image.Format.Rgbaf);
            _layers[i].Fill(Colors.Black);
        }
    }

    private void AddNewPixelsToTree(int layerId)
    {
        var border = layerId == 0 ? StartSize : StartSize * (2 << (layerId - 1));
        
        for (var walkerIndex = 0; walkerIndex < NewPixelsPerLayer * (layerId + 1); walkerIndex++)
        {
            SpawnNewPoint(layerId);

            var stuck = false;
            while (!stuck)
            {
                var vel = Velocity();
                var dir = ((Vector2)_center - _walker).Normalized() * (float)0.3;
                _walker += vel + (Vector2I)dir;

                _walker.X = Math.Clamp(_walker.X, 0, border - 1);
                _walker.Y = Math.Clamp(_walker.Y, 0, border - 1);

                stuck = _tree
                    .Select(point => (point - _walker).Length())
                    .Any(dist => dist <= 1.0);

                if (!stuck) continue;

                _tree.Add(_walker);
            }
        }
    }

    private void SpawnNewPoint(int layerId)
    {
        var border = layerId == 0 ? StartSize : StartSize * (2 << (layerId - 1));
        var direction = _rnd.RandiRange(0, 4);
        _walker = direction switch
        {
            0 => new Vector2I(0, _rnd.RandiRange(0, border - 1)),
            1 => new Vector2I(_rnd.RandiRange(0, border - 1), 0),
            2 => new Vector2I(border - 1, _rnd.RandiRange(0, border - 1)),
            _ => new Vector2I(_rnd.RandiRange(0, border - 1), border - 1)
        };
    }

    private Vector2I Velocity()
    {
        return new Vector2I(
            _rnd.RandiRange(-1, 1),
            _rnd.RandiRange(-1, 1));
    }

    private void DrawTree(int layerId)
    {
        for (var i = 0; i < _tree.Count; i++)
        {
            var point = _tree[i];
            _layers[layerId].SetPixel(point.X, point.Y, Colors.White);
        }
    }
}