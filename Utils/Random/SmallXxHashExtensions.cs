namespace dla_terrain.Utils.Random;

public static class SmallXxHashExtensions
{
    private const float FloatFactor = 1f / 255;

    public static byte GetByte(this SmallXxHash hash, int i)
    {
        return i switch
        {
            0 => (byte)(hash & 255),
            1 => (byte)((hash >> 8) & 255),
            2 => (byte)((hash >> 16) & 255),
            3 => (byte)((hash >> 24) & 255),
            _ => (byte)(hash & 255)
        };
    }

    public static float Float01A(this SmallXxHash hash) => FloatFactor * (hash & 255);
    public static float Float01B(this SmallXxHash hash) => FloatFactor * ((hash >> 8) & 255);
    public static float Float01C(this SmallXxHash hash) => FloatFactor * ((hash >> 16) & 255);
    public static float Float01D(this SmallXxHash hash) => FloatFactor * ((hash >> 24) & 255);
}