using System;
using Godot;

namespace dla_terrain.Utils;

public partial class Camera : Node3D
{
    [Export] private float _speed;
    [Export] private float _sensitivity;

    private Vector2 _lastMouseMotion = Vector2.Zero;
    private float _totalPitch;

    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseMotion eventMouseMotion:
                _lastMouseMotion = eventMouseMotion.Relative;
                break;
            case InputEventMouseButton eventMouseButton:
                Input.MouseMode = eventMouseButton.ButtonIndex switch
                {
                    MouseButton.Right => eventMouseButton.Pressed
                        ? Input.MouseModeEnum.Captured
                        : Input.MouseModeEnum.Visible,
                    _ => Input.MouseMode
                };
                break;
        }
    }

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        UpdateLook();
        UpdateMovement(delta);
    }

    private void UpdateMovement(double delta)
    {
        if (Input.IsActionPressed("up")) GlobalTranslate(Vector3.Up * (float)delta * _speed);
        if (Input.IsActionPressed("down")) GlobalTranslate(Vector3.Down * (float)delta * _speed);
        if (Input.IsActionPressed("forward")) Translate(Vector3.Forward * (float)delta * _speed);
        if (Input.IsActionPressed("back")) Translate(Vector3.Back * (float)delta * _speed);
        if (Input.IsActionPressed("left")) Translate(Vector3.Left * (float)delta * _speed);
        if (Input.IsActionPressed("right")) Translate(Vector3.Right * (float)delta * _speed);
    }

    private void UpdateLook()
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;

        _lastMouseMotion *= _sensitivity;
        var yaw = _lastMouseMotion.X;
        var pitch = _lastMouseMotion.Y;
        _lastMouseMotion = Vector2.Zero;

        pitch = Math.Clamp(pitch, -90f - _totalPitch, 90f - _totalPitch);
        _totalPitch += pitch;

        RotateY(Mathf.DegToRad(-yaw));
        RotateObjectLocal(Vector3.Right, Mathf.DegToRad(-pitch));
    }
}