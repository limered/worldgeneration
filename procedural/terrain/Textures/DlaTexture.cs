using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.procedural.terrain.Textures;

public class DlaTexture
{
    private readonly RandomNumberGenerator _rnd;
    private readonly List<Vector2I> _tree = new();

    private Vector2I _center;
    private Vector2I _walker;

    private Image _image;

    private const int StartSize = 64;

    public DlaTexture()
    {
        _rnd = new RandomNumberGenerator();
        _rnd.Seed = (ulong)GD.Hash("emil");
    }

    public ImageTexture Create()
    {
        _center = new Vector2I(StartSize/2, StartSize/2);
        _tree.Add(_center);

        FillTree();

        _image = Image.Create(StartSize, StartSize, false, Image.Format.Rgbaf);
        _image.Fill(Colors.Black);
        DrawTree();

        return ImageTexture.CreateFromImage(_image);
    }

    private void FillTree()
    {
        for (var walkerIndex = 0; walkerIndex < 300; walkerIndex++)
        {
            SpawnNewPoint();

            var stuck = false;
            while (!stuck)
            {
                var vel = Velocity();
                var dir = ((Vector2)_center - _walker).Normalized() * (float)0.3;
                _walker += vel + (Vector2I)dir;

                _walker.X = Math.Clamp(_walker.X, 0, StartSize-1);
                _walker.Y = Math.Clamp(_walker.Y, 0, StartSize-1);

                stuck = _tree
                    .Select(point => (point - _walker).Length())
                    .Any(dist => dist <= 1.0);

                if (!stuck) continue;
                
                _tree.Add(_walker);
                SpawnNewPoint();
            }
        }
    }

    private void SpawnNewPoint()
    {
        var direction = _rnd.RandiRange(0, 4);
        _walker = direction switch
        {
            0 => new Vector2I(0, _rnd.RandiRange(0, StartSize-1)),
            1 => new Vector2I(_rnd.RandiRange(0, StartSize-1), 0),
            2 => new Vector2I(StartSize-1, _rnd.RandiRange(0, StartSize-1)),
            _ => new Vector2I(_rnd.RandiRange(0, StartSize-1), StartSize-1)
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
            _image.SetPixel(point.X, point.Y, Colors.White - Colors.White/_tree.Count * i);
        }
    }
}