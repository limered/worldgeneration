using Godot;

namespace dla_terrain.Procedural.Terrain.Textures;

public static class Blur2D
{
    public static void BlurImage(Image inputImage, Vector2I kernelSize)
    {
        var width = inputImage.GetWidth();
        var height = inputImage.GetHeight();
        var halfSize = new Vector2I(kernelSize.X / 2, kernelSize.Y / 2);
        var sizeInv = new Vector2(1f / kernelSize.X, 1f / kernelSize.Y);
        var tempImage = Image.Create(width, height, false, Image.Format.Rgbaf);
        
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var sum = 0f;
            for (var i = -halfSize.Y; i <= halfSize.Y; i++)
                if (y + i < 0 || y + i >= height)
                    sum += 0;
                else
                    sum += inputImage.GetPixel(x, y + i).R * sizeInv.Y;

            tempImage.SetPixel(x, y, new Color(sum, sum, sum));
        }

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var sum = 0f;
            for (var i = -halfSize.X; i <= halfSize.X; i++)
                if (x + i < 0 || x + i >= width)
                    sum += 0;
                else
                    sum += tempImage.GetPixel(x + i, y).R * sizeInv.X;

            inputImage.SetPixel(x, y, new Color(sum, sum, sum));
        }
    }
}