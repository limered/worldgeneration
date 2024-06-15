using dla_terrain.SystemBase;
using dla_terrain.ThirdPersonCamera;
using Godot;

namespace dla_terrain.Hero;

public partial class MainHeroNode : Node3D
{
    [Export] private float _speed;
    [Export(PropertyHint.Range, "0, 1, 0.05")] private float _drag;

    private Vector3 _velocity;
    private Vector3 _acceleration;

    private HeroSystem _heroSystem;
    private ThirdPersonCameraSystem _thirdPersonCameraSystem;

    public override void _Ready()
    {
        var systems = GetNode<SystemCollection>("/root/Systems");
        _heroSystem = systems.System<HeroSystem>();
        _thirdPersonCameraSystem = systems.System<ThirdPersonCameraSystem>();
        _thirdPersonCameraSystem.Person = this;
    }

    public override void _Process(double delta)
    {
        UpdateMovement(delta);

        _heroSystem.HeroPosition = Position;
    }

    private void UpdateMovement(double delta)
    {
        _acceleration += _thirdPersonCameraSystem.PersonMovementDirection() * _speed;
        
        var tempVelocity = _velocity + _acceleration * (float)delta;
        Position += (tempVelocity + _velocity) * 0.5f * (float)delta;
        _velocity = tempVelocity * _drag;

        _acceleration = Vector3.Zero;
    }
}