using System.Collections.Generic;
using System.Linq;
using Godot;

namespace dla_terrain.Procedural.Terrain;

public class Map
{
    private const int K = 30;

    private readonly ChunkCell[] _chunks;
    private readonly MapInitialization _mapData;
    private readonly int R;
    private readonly int R2;

    private int _pChunks;

    public Map(MapInitialization mapData)
    {
        _mapData = mapData;
        R = (int)(_mapData.CellSize * Mathf.Sqrt(2));
        R2 = R * 2;
        _chunks = new ChunkCell[_mapData.MaxChunksCount];
    }

    public void GenerateInitialChunks()
    {
        var border = new Queue<Vector2I>();
        border.Enqueue(new Vector2I(0, 0));

        for (var i = 0; i < _mapData.InitialChunkCount; i++)
        {
            var center = border.Dequeue();
            var neighbours = NeighbourCoordinates(center);
            var chunk = new ChunkCell(
                    center,
                    _mapData.MasterSeed,
                    _mapData.CellSize,
                    R, R2, K)
                .ReGenerate(NeighbourCenters(center), R);

            _chunks[_pChunks++] = chunk;

            for (var n = 0; n < 8; n++)
                if (_chunks.Where(c => c != null).All(c => c.Coordinate != neighbours[n]))
                    border.Enqueue(neighbours[n]);
        }
    }

    private Vector3[] NeighbourCenters(Vector2I c)
    {
        return new[]
            {
                FindCell(new Vector2I(-1, -1) * _mapData.CellSize + c),
                FindCell(new Vector2I(-1, 0) * _mapData.CellSize + c),
                FindCell(new Vector2I(-1, +1) * _mapData.CellSize + c),
                FindCell(new Vector2I(0, -1) * _mapData.CellSize + c),
                FindCell(new Vector2I(0, +1) * _mapData.CellSize + c),
                FindCell(new Vector2I(+1, -1) * _mapData.CellSize + c),
                FindCell(new Vector2I(+1, 0) * _mapData.CellSize + c),
                FindCell(new Vector2I(+1, +1) * _mapData.CellSize + c)
            }
            .Where(chunk => chunk != null)
            .Select(chunk => chunk.CenterPoint)
            .ToArray();
    }

    private ChunkCell FindCell(Vector2I coord)
    {
        return _chunks
            .Where(c => c != null)
            .FirstOrDefault(c => c.Coordinate == coord);
    }

    private Vector2I[] NeighbourCoordinates(Vector2I c)
    {
        return new[]
        {
            new Vector2I(-1, -1) * _mapData.CellSize + c,
            new Vector2I(-1, 0) * _mapData.CellSize + c,
            new Vector2I(-1, +1) * _mapData.CellSize + c,
            new Vector2I(0, -1) * _mapData.CellSize + c,
            new Vector2I(0, +1) * _mapData.CellSize + c,
            new Vector2I(+1, -1) * _mapData.CellSize + c,
            new Vector2I(+1, 0) * _mapData.CellSize + c,
            new Vector2I(+1, +1) * _mapData.CellSize + c
        };
    }

    public void Update(Node3D parent)
    {
        foreach (var chunk in _chunks)
        {
            if (chunk is null or { IsRendered: true }) continue;
            chunk.IsRendered = true;
            var chunkScene = GD.Load<PackedScene>("res://Scenes/chunk_center.tscn");
            var sceneInstance = chunkScene.Instantiate<Node3D>();
            parent.AddChild(sceneInstance);
            sceneInstance.Translate(chunk.CenterPoint);
        }
    }
}