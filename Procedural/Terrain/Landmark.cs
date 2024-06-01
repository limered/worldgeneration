using dla_terrain.Procedural.Terrain.DLA;
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
    private readonly DlaAlgorithm _dla;
    private readonly SmallXxHash[] _landmarkGenome = new SmallXxHash[3];
    private FastNoiseLite _heightNoise;

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

        _dla = new DlaAlgorithm(_landmarkGenome[(int)GenomeMap.DLATreeSeed]);
    }

    public Vector3 LandmarkPosition { get; private set; }
    public Vector2I CellIndex { get; }
    public Vector3 CellCoordinate { get; }

    public float HeightMultiplier()
    {
        if (_heightNoise == null) return _landmarkGenome[(int)GenomeMap.CenterPointHeight].Float01A() * 2f - 1f;

        _heightNoise.Seed = (int)(uint)_landmarkGenome[(int)GenomeMap.CenterPointHeight];
        return _heightNoise.GetNoise2D(CellCoordinate.X, CellCoordinate.Z) * 2f;
    }

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

        _tex = _dla.Run(10);
        return _tex;
    }

    public Landmark AddHeightNoise(FastNoiseLite heightNoise)
    {
        _heightNoise = heightNoise;
        return this;
    }
}