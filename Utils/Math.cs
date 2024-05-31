using System;

namespace dla_terrain.Utils;

public static class Math
{
    public static float ExpDecay(float a, float b, float decay = 16f, float dt = 1f)
    {
        return b + (a - b) * MathF.Exp(-decay * dt);
    }
}