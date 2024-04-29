using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain.Textures;

public class DlaTexture
{
    private readonly RandomNumberGenerator _rnd;
    private readonly List<Vector2I> _tree = new();

    private Vector2I _center;
    private Vector2I _walker;

    private readonly Image[] _images = new Image[5];

    private const int StartSize = 8;

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

        for (var i = 0; i < 5; i++)
        {
            var size = i == 0 ? StartSize : StartSize * (2 << (i - 1));
            GD.Print(size);
            _images[i] = Image.Create(size, size, false, Image.Format.Rgbaf);
            _images[i].Fill(Colors.Black);
        }
        
        DrawTree(0);

        return ImageTexture.CreateFromImage(_images[0]);
    }

    private void FillTree()
    {
        for (var walkerIndex = 0; walkerIndex < 20; walkerIndex++)
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

    private void DrawTree(int id)
    {
        for (var i = 0; i < _tree.Count; i++)
        {
            var point = _tree[i];
            _images[id].SetPixel(point.X, point.Y, Colors.White - Colors.White/_tree.Count * i);
        }
    }
}