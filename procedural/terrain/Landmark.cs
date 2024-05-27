using System.Collections.Generic;
using dla_terrain.Procedural.Terrain.Textures;
using dla_terrain.Utils.Godot;
using dla_terrain.Utils.Random;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public enum GenomeMap
{
    CenterPointCoords,
    DLATreeSeed,
    CenterPointHeight
}

public record Landmark
{
    private readonly SmallXxHash _baseHash;
    private readonly int _cellSize;
    private readonly DlaTree _dla;
    private readonly SmallXxHash[] _landmarkGenome = new SmallXxHash[3];

    private ImageTexture _tex;

    public Landmark(
        Vector2I cellIndex,
        int cellSize,
        int masterSeed)
    {
        CellIndex = cellIndex;
        CellCoordinate = new Vector3(CellIndex.X, 0, CellIndex.Y) * cellSize;
        _cellSize = cellSize;

        var baseHash = SmallXxHash.Seed(masterSeed).Eat(CellIndex.X).Eat(CellIndex.Y);

        _landmarkGenome[(int)GenomeMap.CenterPointCoords] = baseHash;
        _landmarkGenome[(int)GenomeMap.DLATreeSeed] = baseHash.Eat(0);
        _landmarkGenome[(int)GenomeMap.CenterPointHeight] = baseHash.Eat((int)GenomeMap.CenterPointHeight);

        var rnd = new RandomNumberGenerator();
        rnd.Seed = _landmarkGenome[(int)GenomeMap.DLATreeSeed];
        _dla = new DlaTree(rnd);
    }

    public Vector3 LandmarkPosition { get; private set; }
    public Vector2I CellIndex { get; }
    public Vector3 CellCoordinate { get; }

    public float HeightMultiplier => _landmarkGenome[(int)GenomeMap.CenterPointHeight].Float01A() * 2f - 1f;

    public Landmark Generate()
    {
        var x = _landmarkGenome[(int)GenomeMap.CenterPointCoords].Float01A();
        var y = _landmarkGenome[(int)GenomeMap.CenterPointCoords].Float01B();
        LandmarkPosition = new Vector3(x * _cellSize, 0, y * _cellSize);

        return this;
    }

    public ImageTexture GenerateGroundTexture()
    {
        if (_tex != null) return _tex;

        const int baseSize = 8;

        var image = Image.Create(baseSize, baseSize, false, Image.Format.Rf);
        image.Fill(Colors.Black);

        var startPixel = (LandmarkPosition / _cellSize * (baseSize - 1)).XZ();
        _dla.Reset(8, 10, 0.3f, 0.5f, 1.5f, startPixel);
        _dla.AddNewPixelsToTree(0);
        _dla.CalculateHeights();
        DrawTree(_dla.Points, image);
        image.Resize(baseSize * 2, baseSize * 2);
        Blur2D.BlurImage(image, new Vector2I(3, 3));

        for (var i = 1; i < 5; i++)
        {
            _dla.ExpandTree();
            _dla.FillGaps();
            _dla.AddNewPixelsToTree(i);
            _dla.CalculateHeights();
            DrawTree(_dla.Points, image);
            image.Resize(baseSize * (2 << i), baseSize * (2 << i));
            Blur2D.BlurImage(image, new Vector2I(7, 7));
        }

        var finalSize = baseSize * (2 << 4); // + 32;
        var final = Image.Create(finalSize, finalSize, false, Image.Format.Rf);
        final.BlendRect(image, new Rect2I(0, 0, image.GetWidth(), image.GetHeight()), 0.ToVector2I());
        // Blur2D.BlurImage(final, new Vector2I(8, 8));
        final.Resize(1024, 1024);
        _tex = ImageTexture.CreateFromImage(final);
        return _tex;
    }

    private static void DrawTree(IReadOnlyList<DlaPoint> points, Image image)
    {
        var width = image.GetWidth();
        var height = image.GetHeight();
        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            if (point.Position.X >= width ||
                point.Position.X < 0 ||
                point.Position.Y >= height ||
                point.Position.Y < 0) continue;

            // var col = 1f - 1f / (1f + point.Height);
            // var col = 1f / 1f + Mathf.Exp(-point.Height);
            // var col = (1 + Mathf.Tanh(point.Height)) / 2f * 0.5f + 0.5f;
            var col = Logistic(point.Height, 0.2f, 0f);
            image.SetPixel((int)point.Position.X, (int)point.Position.Y, new Color(col, 0, 0));
        }
    }

    private static float Logistic(float x, float k = 1, float x0 = 0)
    {
        return 1f / (1f + Mathf.Exp(-k * (x - x0)));
    }
}