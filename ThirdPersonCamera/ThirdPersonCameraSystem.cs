using dla_terrain.Hero;
using dla_terrain.SystemBase;
using dla_terrain.Utils;
using Godot;

namespace dla_terrain.ThirdPersonCamera;

public class ThirdPersonCameraSystem : ISystem
{
    public MainHeroNode Person { private get; set; }
    public CameraNode Camera { private get; set; }

    public Vector3 CameraPosition(double dt, float cameraDistance, float damping)
    {
        var targetPosition = Person?.Position ?? Vector3.Zero;
        var movementTarget = targetPosition + Camera.Transform.Basis.Z * cameraDistance;

        return (Camera.Position - movementTarget).Length() < 0.001f
            ? Camera.Position
            : Math.ExpDecay(movementTarget, Camera.Position, damping, (float)dt);
    }

    public Vector3 PersonMovementDirection()
    {
        var direction = Camera?.Position.DirectionTo(Person.Position) ?? Vector3.Zero;
        direction.Y = 0;

        var forwardForce = direction;
        if (Input.IsActionPressed("forward")) forwardForce *= 1;
        else if (Input.IsActionPressed("back")) forwardForce *= -1;
        else forwardForce = Vector3.Zero;

        var sideForce = direction.Rotated(Vector3.Up, Mathf.Pi * 0.5f);
        if (Input.IsActionPressed("right")) sideForce *= -1;
        else if (Input.IsActionPressed("left")) sideForce *= 1;
        else sideForce = Vector3.Zero;

        return (forwardForce + sideForce).Normalized();
    }
}