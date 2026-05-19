using System.Numerics;

namespace FlatRedBall2.Math;

/// <summary>
/// Stateless second-order kinematic integrator used by Entity and Camera.
/// Integration order: position updated with current velocity + half-acceleration, then velocity updated.
/// </summary>
public static class KinematicIntegrator
{
    /// <summary>
    /// Integrates position and velocity for one frame using second-order kinematics.
    /// </summary>
    /// <param name="position">World position (modified in place).</param>
    /// <param name="velocity">Current velocity (modified in place).</param>
    /// <param name="acceleration">Constant acceleration this frame.</param>
    /// <param name="drag">Fractional velocity reduction per second (0 = no drag).</param>
    /// <param name="dt">Frame time in seconds.</param>
    public static void Integrate(ref Vector2 position, ref Vector2 velocity, Vector2 acceleration, float drag, float dt)
    {
        float halfDt2 = 0.5f * dt * dt;
        position += velocity * dt + acceleration * halfDt2;
        velocity += acceleration * dt;
        velocity -= velocity * (drag * dt);
    }

    /// <summary>
    /// Integrates rotation and rotation velocity for one frame using second-order kinematics.
    /// Pass <c>rotationAcceleration = 0</c> for first-order (constant-velocity) rotation.
    /// </summary>
    /// <param name="rotation">Current rotation in radians (modified in place).</param>
    /// <param name="rotationVelocity">Current angular velocity in radians/second (modified in place).</param>
    /// <param name="rotationAcceleration">Angular acceleration in radians/second².</param>
    /// <param name="dt">Frame time in seconds.</param>
    public static void IntegrateRotation(ref float rotation, ref float rotationVelocity, float rotationAcceleration, float dt)
    {
        float halfDt2 = 0.5f * dt * dt;
        rotation += rotationVelocity * dt + rotationAcceleration * halfDt2;
        rotationVelocity += rotationAcceleration * dt;
    }
}
