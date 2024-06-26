using dla_terrain.SystemBase;
using dla_terrain.ThirdPersonCamera;
using Godot;

namespace dla_terrain.Utils;

public enum CameraType
{
    Free,
    ThirdPerson
}

public partial class CameraNode : Node3D
{
    [Export] private float _cameraDistance;
    [Export] private float _cameraSide;
    [Export] private CameraType _cameraType;

    [ExportCategory("3rd Person")] [Export(PropertyHint.Range, "1, 500")]
    private float _damping;

    private Vector2 _lastMouseMotion = Vector2.Zero;
    [Export] private float _sensitivity;
    [Export] private Vector3 _shoulderOffset;
    [Export] private float _speed;
    private ThirdPersonCameraSystem _system;
    private float _totalPitch;
    [Export] private float _verticalArmLength;

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
        _system = GetNode<SystemCollection>("/root/Systems")
            .System<ThirdPersonCameraSystem>();
        _system.Camera = this;
    }

    public override void _Process(double delta)
    {
        if (_cameraType == CameraType.Free)
        {
            UpdateLook();
            UpdateMovement(delta);
        }
        else if (_cameraType == CameraType.ThirdPerson)
        {
            UpdateLook();
            UpdatePositionFromTarget((float)delta);
        }
    }

    private void UpdatePositionFromTarget(float dt)
    {
        Position = _system.CameraPosition(dt, _cameraDistance, _damping);
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

        pitch = Mathf.Clamp(pitch, -90f - _totalPitch, 90f - _totalPitch);
        _totalPitch += pitch;

        RotateY(Mathf.DegToRad(-yaw));
        RotateObjectLocal(Vector3.Right, Mathf.DegToRad(-pitch));
    }
}