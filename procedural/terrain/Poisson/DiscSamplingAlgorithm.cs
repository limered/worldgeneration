using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain.Poisson;

public class DiscSamplingAlgorithm
{
    private const int Resolution = 1024;
    private const int HalfResolution = Resolution / 2;
    private const int R = 40;
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
        return x + y * _cellCount;
    }

    private int ToCellIndex(Vector2 v)
    {
        var x = v.X / _cellSize;
        var y = v.Y / _cellSize;
        return Vector2Index((int)x, (int)y);
    }

    public ImageTexture Create()
    {
        IsGenerating = true;
        Init();
        
        while (_activeList.Any())
        {
            var i = _rnd.RandiRange(0, _activeList.Count - 1);
            var xI = _grid[_activeList[i]];
            var pointAdded = false;
            for (var k = 0; k < K; k++)
            {
                var rad = _rnd.RandfRange(0, Mathf.Tau);
                var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).Normalized();
                dir *= _rnd.RandfRange(R, R2);
                var newPoint = dir + xI;
                var cellIndex = ToCellIndex(newPoint);
                if (cellIndex >= _grid.Length ||
                    cellIndex < 0 ||
                    _grid[cellIndex] != _empty) continue;

                pointAdded = true;
                _grid[cellIndex] = newPoint;
                _activeList.Add(cellIndex);
            }

            // ToDo: Check if all neighbours are set
            if (!pointAdded) _activeList.RemoveAt(i);
        }

        for (var p = 0; p < _grid.Length; p++)
        {
            var point = _grid[p];
            if (point != _empty &&
                point.X is < Resolution and >= 0 &&
                point.Y is < Resolution and >= 0)
                _image.SetPixel((int)point.X, (int)point.Y, Color.Color8(255, 0, 0));
        }

        IsGenerating = false;
        return ImageTexture.CreateFromImage(_image);
    }
}