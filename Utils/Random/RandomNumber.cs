using Godot;

namespace dla_terrain.Utils.Random;

public class RandomNumber
{
    private readonly SmallXxHash _hash;
    private long _counter;

    public RandomNumber(int? seed)
    {
        _hash = SmallXxHash.Seed(seed ?? GD.Hash(GD.Randi()));
    }

    public void Reset()
    {
        _counter = 0;
    }

    public uint LastUint()
    {
        return _hash;
    }

    public int LastInt()
    {
        return (int)(uint)_hash;
    }

    public float LastFloat()
    {
        var v = (uint)_hash;
        var f = (int)v & 255;
        return f * (1f / 255);
    }
    
    public uint Uint()
    {
        return _hash.Eat((byte)_counter++);
    }

    public uint Range(uint start, uint stop)
    {
        return Uint() % (stop - start) + start;
    }

    public int Int()
    {
        return (int)(uint)_hash.Eat((byte)_counter++);
    }

    public int Range(int start, int end)
    {
        var v = _hash.Eat((byte)_counter++);
        return (int)(v % (end - start) + start);
    }

    public float Float()
    {
        var v = (uint)_hash.Eat((byte)_counter++);
        var f = (int)v & 255;
        return f * (1f / 255);
    }

    public float Range(float start, float end)
    {
        return Float() * (end - start) + start;
    }
}