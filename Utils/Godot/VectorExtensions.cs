using Godot;

namespace dla_terrain.Utils.Godot;

public static class VectorExtensions
{
    // ReSharper disable once InconsistentNaming
    public static Vector2 XZ(this Vector3 v)
    {
        return new Vector2(v.X, v.Z);
    }

    public static Vector2I XZi(this Vector3 v)
    {
        return new Vector2I((int)v.X, (int)v.Z);
    }

    public static Vector2I ToVector2I(this int i)
    {
        return new Vector2I(i, i);
    }
    
    public static Vector2I ToVector2I(this Vector2 v)
    {
        return new Vector2I((int)v.X, (int)v.Y);
    }
}