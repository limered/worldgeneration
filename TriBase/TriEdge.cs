using System.Runtime.InteropServices;

namespace dla_terrain.TriBase;

[StructLayout(LayoutKind.Sequential)]
public readonly struct TriEdge
{
    public readonly int q;
    public readonly int r;
    public readonly TriDirection d;

    public TriEdge(int q, int r, TriDirection d)
    {
        this.q = q;
        this.r = r;
        this.d = d;
    }
}