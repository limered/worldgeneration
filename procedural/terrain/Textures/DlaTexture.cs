using Godot;

namespace dla_terrain.procedural.terrain.Textures;

public class DlaTexture
{
    public ImageTexture Create()
    {
        var image = Image.Create(8, 8, false, Image.Format.Rgbaf);
        image.Fill(Colors.Black);
        image.SetPixel(4, 4, Colors.White);
        
        return ImageTexture.CreateFromImage(image);
    }
}