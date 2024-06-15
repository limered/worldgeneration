using System;
using System.Runtime.InteropServices;
using Godot;

namespace dla_terrain.TriBase;

[StructLayout(LayoutKind.Sequential)]
public readonly struct TriFace
{
    public readonly int q;
    public readonly int r;
    public readonly TriType p;

    public TriFace(int q, int r, TriType p)
    {
        this.q = q;
        this.r = r;
        this.p = p;
    }

    public bool PointsUp => p == TriType.L;

    public int C => p == TriType.L ? 2 - q - r : 1 - q - r;

    public static TriFace FromWorld(Vector2 coord, float height)
    {
        var r = coord.Y / height;
        var q = coord.X * TriMath.ConversionFactor / height - 0.5f * r;
        var c = Frac(q) + Frac(r) < 1f ? TriType.L : TriType.R;

        return new TriFace((int)Math.Floor(q), (int)Math.Floor(r), c);
    }

    public static float Frac(float a)
    {
        return (float)(a - Math.Truncate(a));
    }
}