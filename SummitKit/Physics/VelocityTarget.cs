using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

public class VelocityTarget(Func<Vector2> to, Func<Vector2> currentPosition, Func<Vector2> getter, Action<Vector2> setter, Action<ITarget<Vector2>> callback, Func<bool> continues, float acceleration = 5, float target = 120, float slowRadius = 20) : ITarget<Vector2> {
    public float TargetSpeed { get => target * 100; }
    public float Acceleration { get => acceleration * 1000; }
    public float SlowRadius { get; set; } = slowRadius;

    private readonly Func<Vector2> GetVelocity = getter;
    private readonly Action<Vector2> SetVelocity = setter;
    private readonly Func<Vector2> GetTo = to;
    private readonly Func<Vector2> GetFrom = currentPosition;
    private readonly Func<bool> ShouldContinue = continues;
    public VelocityTarget(Func<Vector2> to, Entity entity, Action<ITarget<Vector2>> callback = null, Func<bool> continues = null, float accel = 5f, float target = 10, float slowRadius = 20) : this(to, () => entity.Position, () => entity.Velocity, (vel) => entity.Velocity = vel, callback, continues, accel, target, slowRadius)
    {
    }

    public Vector2 From { get => GetFrom(); set { } }
    public Vector2 To { get => GetTo(); }
    public Action<ITarget<Vector2>> Callback { get; set; } = null;
    public bool IsComplete => ((To - From).Length() < 0.01F) && Callback is null && (ShouldContinue is null || !ShouldContinue());

    public void Update(GameTime time)
    {
        Vector2 currentVelocity = GetVelocity();
        Vector2 direction = To - From;
        float distance = direction.Length();

        // If we're essentially at the target, stop and invoke the callback (if any).
        if (distance < 0.01f)
        {
            if (Callback is not null)
            {
                Callback(this);
                Callback = null;
            }
            SetVelocity(Vector2.Zero);
            return;
        }

        // Normalize direction for steering calculations
        direction /= distance;

        // Use time delta so acceleration is framerate-independent
        float dt = (float)time.ElapsedGameTime.TotalSeconds;
        if (dt <= 0f) dt = 1f / 60f;

        // Arrival behaviour:
        // Reduce desired speed as we approach the target so we slow smoothly.
        // slowRadius determines how far out we start slowing. Use TargetSpeed as a base.
        float slowRadius = Math.Max(SlowRadius, 1f);
        float desiredSpeed = (distance < slowRadius) ? SlowRadius * (distance / slowRadius) : TargetSpeed;
        Vector2 desiredVelocity = direction * desiredSpeed;

        // Compute steering (desired change in velocity) and limit it by Acceleration * dt
        Vector2 steering = desiredVelocity - currentVelocity;
        float maxAccelThisFrame = Acceleration * dt;
        float steeringMag = steering.Length();
        if (steeringMag > maxAccelThisFrame && steeringMag > 0f)
        {
            steering = Vector2.Normalize(steering) * maxAccelThisFrame;
        }

        Vector2 newVelocity = currentVelocity + steering;

        // Clamp to maximum speed
        float newSpeed = newVelocity.Length();
        if (newSpeed > TargetSpeed)
        {
            newVelocity = Vector2.Normalize(newVelocity) * TargetSpeed;
        }

        SetVelocity(newVelocity);
    }
}
