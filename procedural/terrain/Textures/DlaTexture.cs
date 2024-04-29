using System.Collections.Generic;
using Godot;

namespace dla_terrain.procedural.terrain.Textures;

public class DlaTexture
{
    private readonly RandomNumberGenerator _rnd;
    private readonly List<Vector2I> _tree = new();

    private Image _image;
    
    public DlaTexture()
    {
        _rnd = new RandomNumberGenerator();
        _rnd.Seed = (ulong)GD.Hash("emil");
    }
    
    public ImageTexture Create()
    {
        _tree.Add(new Vector2I(3, 3));
        
        _image = Image.Create(8, 8, false, Image.Format.Rgbaf);
        _image.Fill(Colors.Black);
        DrawTree();
        
        return ImageTexture.CreateFromImage(_image);
    }

    private void DrawTree()
    {
        for (var i = 0; i < _tree.Count; i++)
        {
            var point = _tree[i];
            _image.SetPixel(point.X, point.Y, Colors.White);
        }
    }
}