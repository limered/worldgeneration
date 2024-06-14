using dla_terrain.SystemBase;
using Godot;

namespace dla_terrain.ThirdPersonCamera;

public class ThirdPersonCameraSystem : ISystem
{
    public Node3D Person { get; set; }
    public Node3D Camera { get; set; }
}