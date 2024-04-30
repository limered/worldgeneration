using Godot;

namespace dla_terrain.Procedural.Terrain.Textures;

public class Blur2D
{
    public Image BlurImage(Image inputImage, Vector2I size)
    {
        var width = inputImage.GetWidth();
        var height = inputImage.GetHeight();
        var yHalfSize = size.Y / 2;
        var xHalfSize = size.X / 2;
        var sizeInv = new Vector2(1f / size.X, 1f / size.Y);
        var tempImage = Image.Create(width, height, false, Image.Format.Rgbaf);
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var sum = 0f;
            for (var i = -yHalfSize; i <= yHalfSize; i++)
            {
                if (y + i < 0 || y + i >= height)
                {
                    sum += 0;
                }
                else
                {
                    sum += inputImage.GetPixel(x, y + i).R * sizeInv.Y;
                }
            }

            tempImage.SetPixel(x, y, new Color(sum, sum, sum));
        }

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var sum = 0f;
            for (var i = -xHalfSize; i <= xHalfSize; i++)
            {
                if (x + i < 0 || x + i >= width)
                {
                    sum += 0;
                }
                else
                {
                    sum += tempImage.GetPixel(x + i, y).R * sizeInv.X;
                }
            }

            inputImage.SetPixel(x, y, new Color(sum, sum, sum));
        }
        
        return inputImage;
    }
}