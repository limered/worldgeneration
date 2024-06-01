using System.Collections.Generic;
using dla_terrain.Procedural.Terrain.Textures;
using dla_terrain.Utils.Godot;
using Godot;

namespace dla_terrain.Procedural.Terrain.DLA;

public class DlaImageTexture
{
    private readonly Image _image;

    public DlaImageTexture(int size)
    {
        _image = Image.Create(size, size, false, Image.Format.Rf);
    }

    public DlaImageTexture Resize(int size)
    {
        _image.Resize(size, size);
        return this;
    }

    public DlaImageTexture Blur(int kernelSize)
    {
        Blur2D.BlurImage(_image, kernelSize.ToVector2I());
        return this;
    }

    public DlaImageTexture DrawPixels(
        IReadOnlyList<Particle> points,
        uint[] heights,
        Vector2I center, 
        float k = 1, float x0 = 0)
    {
        var width = _image.GetWidth();
        var height = _image.GetHeight();
        for (var i = 0; i < points.Count; i++)
        {
            var position = points[i].Position.ToVector2I() + center;
            if (position.X >= width ||
                position.X < 0 ||
                position.Y >= height ||
                position.Y < 0) continue;

            var col = Logistic(heights[i], k, x0);
            _image.SetPixel(position.X, position.Y, new Color(col, 0, 0));
        }

        return this;
    }

    public DlaImageTexture Reset(int size)
    {
        _image.Resize(size, size);
        _image.Fill(Colors.Black);
        return this;
    }

    public ImageTexture ImageTexture()
    {
        return Godot.ImageTexture.CreateFromImage(_image);
    }

    private static float Logistic(float x, float k = 1, float x0 = 0)
    {
        return 1f / (1f + Mathf.Exp(-k * (x - x0)));
    }
}