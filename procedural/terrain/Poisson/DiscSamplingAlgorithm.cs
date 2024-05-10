using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain.Poisson;

public class DiscSamplingAlgorithm
{
    private const int Resolution = 1024;
    private const int HalfResolution = Resolution / 2;
    private const int R = 30;
    private const int R2 = R * 2;
    private const int K = 30;
    private readonly Vector2 _empty = new(-1, -1);

    private readonly RandomNumberGenerator _rnd;

    private readonly Image _image = Image
        .Create(Resolution, Resolution, false, Image.Format.R8);

    private readonly int _cellSize = (int)(R / Math.Sqrt(2));
    private readonly int _cellCount;
    private readonly Vector2[] _grid;
    private List<int> _activeList;

    public DiscSamplingAlgorithm(string seed)
    {
        _rnd = new RandomNumberGenerator();
        if (!string.IsNullOrEmpty(seed)) _rnd.Seed = (ulong)GD.Hash(seed);

        _cellCount = Resolution / _cellSize;
        _grid = new Vector2[_cellCount * _cellCount];
    }

    private void Init()
    {
        for (var i = 0; i < _grid.Length; i++)
            _grid[i] = _empty;
        _activeList = new List<int>();
        _grid[Vector2Index(_cellCount / 2, _cellCount / 2)] = new Vector2(HalfResolution, HalfResolution);
        _activeList.Add(Vector2Index(_cellCount / 2, _cellCount / 2));
        _image.Fill(Colors.Black);
    }

    public bool IsGenerating { get; private set; }

    private int Vector2Index(int x, int y)
    {
        x = Math.Clamp(x, 0, _cellCount - 1);
        y = Math.Clamp(y, 0, _cellCount - 1);
        return x + y * _cellCount;
    }

    private Vector2I ToCell(Vector2 v)
    {
        return new Vector2I((int)(v.X / _cellSize), (int)(v.Y / _cellSize));
    }

    private int ToCellIndex(Vector2 v)
    {
        var vI = ToCell(v);
        return Vector2Index(vI.X, vI.Y);
    }

    private Vector2[] Neighbours(Vector2I v)
    {
        return new[]
        {
            _grid[Vector2Index(v.X - 1, v.Y - 1)],
            _grid[Vector2Index(v.X - 1, v.Y)],
            _grid[Vector2Index(v.X - 1, v.Y + 1)],
            _grid[Vector2Index(v.X, v.Y - 1)],
            _grid[Vector2Index(v.X, v.Y + 1)],
            _grid[Vector2Index(v.X + 1, v.Y - 1)],
            _grid[Vector2Index(v.X + 1, v.Y)],
            _grid[Vector2Index(v.X + 1, v.Y + 1)]
        };
    }

    public ImageTexture Create()
    {
        IsGenerating = true;
        Init();

        while (_activeList.Any())
        {
            var i = _rnd.RandiRange(0, _activeList.Count - 1);
            var xI = _grid[_activeList[i]];
            var found = false;
            for (var k = 0; k < K; k++)
            {
                var theta = _rnd.RandfRange(0f, (float)Math.Tau);
                var dir = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
                dir *= _rnd.RandfRange(R, R2);
                var sample = dir + xI;
                var sampleCellIndex = ToCellIndex(sample);
                if (sampleCellIndex >= _grid.Length ||
                    sampleCellIndex < 0 ||
                    _grid[sampleCellIndex] != _empty) continue;

                var neighbours = Neighbours(ToCell(sample));
                var ok = true;
                var n = 0;
                while (ok && n < 8) ok &= (sample - neighbours[n++]).Length() >= R;

                if (!ok) continue;
                found = true;
                _grid[sampleCellIndex] = sample;
                _activeList.Add(sampleCellIndex);
            }

            if (!found) _activeList.RemoveAt(i);
        }

        foreach (var point in _grid)
            if (point != _empty &&
                point.X is < Resolution and >= 0 &&
                point.Y is < Resolution and >= 0)
                _image.SetPixel((int)point.X, (int)point.Y, Color.Color8(255, 255, 255));

        IsGenerating = false;
        return ImageTexture.CreateFromImage(_image);
    }
}