using System;
using Godot;

namespace dla_terrain.Utils;

public static class Math
{
    public static float ExpDecay(float a, float b, float decay = 16f, float dt = 1f)
    {
        return b + (a - b) * MathF.Exp(-decay * dt);
    }

    public static Vector2 ExpDecay(Vector2 a, Vector2 b, float decay = 16f, float dt = 1f)
    {
        return b + (a - b) * MathF.Exp(-decay * dt);
    }

    public static Vector3 ExpDecay(Vector3 a, Vector3 b, float decay = 16f, float dt = 1f)
    {
        return b + (a - b) * MathF.Exp(-decay * dt);
    }
}