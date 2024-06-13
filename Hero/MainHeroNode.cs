using dla_terrain.SystemBase;
using dla_terrain.Utils;
using Godot;

namespace dla_terrain.Hero;

public partial class MainHeroNode : Node3D
{
    [Export] private float _speed;
    [Export(PropertyHint.Range, "0, 1, 0.05")] private float _drag;
    [Export] private Node3D _camera;

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
        var direction = _camera?.Position.DirectionTo(Position) ?? Vector3.Zero;
        direction.Y = 0;

        var forwardForce = direction;
        if (Input.IsActionPressed("forward")) forwardForce *= 1;
        else if (Input.IsActionPressed("back")) forwardForce *= -1;
        else forwardForce = Vector3.Zero;

        var sideForce = direction.Rotated(Vector3.Up, Mathf.Pi*0.5f);
        if (Input.IsActionPressed("right")) sideForce *= -1;
        else if (Input.IsActionPressed("left")) sideForce *= 1;
        else sideForce = Vector3.Zero;
        
        forwardForce = (forwardForce + sideForce).Normalized() * _speed;
        
        _acceleration += forwardForce;
        var tempVelocity = _velocity + _acceleration * (float)delta;
        Position += (tempVelocity + _velocity) * 0.5f * (float)delta;
        _velocity = tempVelocity * _drag;

        _acceleration = Vector3.Zero;
    }
}