using dla_terrain.SystemBase;
using Godot;

namespace dla_terrain.Hero;

public partial class MainHero : Node3D
{
    [Export] private float _speed;
    [Export] private float _drag;

    private Vector3 _velocity;
    private Vector3 _acceleration;

    private SystemCollection _systems;

    public override void _Ready()
    {
        _systems = GetNode<SystemCollection>("/root/Systems");
    }

    public override void _Process(double delta)
    {
        UpdateMovement(delta);

        _systems.System<HeroSystem>().HeroPosition = Position;
    }

    private void UpdateMovement(double delta)
    {
        var movementForce = new Vector3();
        if (Input.IsActionPressed("forward")) movementForce.Z -= 1;
        else if (Input.IsActionPressed("back")) movementForce.Z += 1;

        if (Input.IsActionPressed("right")) movementForce.X += 1;
        else if (Input.IsActionPressed("left")) movementForce.X -= 1;
        movementForce *= _speed;
        
        _acceleration += movementForce;
        var tempVelocity = _velocity + _acceleration * (float)delta;
        Position += (tempVelocity + _velocity) * 0.5f * (float)delta;
        _velocity = tempVelocity * _drag;
        
        
        
        _acceleration = Vector3.Zero;
    }
}