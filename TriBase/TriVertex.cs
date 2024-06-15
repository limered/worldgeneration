using System;
using System.Runtime.InteropServices;
using Godot;

namespace dla_terrain.TriBase;

[StructLayout(LayoutKind.Sequential)]
public readonly struct TriVertex
{
    public readonly int q;
    public readonly int r;

    public TriVertex(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public Vector2 ToWorld(float height)
    {
        return ToWorld(q, r, height);
    }

    public static Vector2 ToWorld(int q, int r, float height)
    {
        return new Vector2(
            (q + r * 0.5f) * height * TriMath.ConversionFactorInv,
            r * height);
    }

    public static TriVertex FromWorld(Vector2 worldLocation, float height)
    {
        var r = worldLocation.Y / height;
        var q = worldLocation.X * TriMath.ConversionFactor / height - 0.5f * r;

        return new TriVertex((int)Math.Floor(q), (int)Math.Floor(r));
    }

    public override string ToString()
    {
        return $"({q}, {r})";
    }
}