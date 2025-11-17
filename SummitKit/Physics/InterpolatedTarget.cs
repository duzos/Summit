using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SummitKit.Physics;

/// <summary>
/// For moving an object to a target position over a specified duration.
/// </summary>
public class InterpolatedTarget(Vector2 to, Vector2 from, Action<Vector2> setter, TimeSpan duration, TimeSpan delay, InterpolationType type = InterpolationType.Smooth, Action<ITarget> callback = null) : ITarget
{
    public Vector2 To { get; init; } = to;
    public Vector2 From { get; set; } = from;
    public TimeSpan Duration { get; init; } = duration + delay;
    public TimeSpan Delay { get; init; } = delay;
    public Vector2 Position { get; protected set; } = from;
    public Action<Vector2> SetPosition { get; set; } = setter;
    public InterpolationType Type { get; init; } = type;
    public Action<ITarget> Callback { get; set; } = callback;
    private TimeSpan _elapsed = TimeSpan.Zero;

    public bool IsComplete => _elapsed >= Duration && Callback is null;
    public void Update(GameTime time)
    {
        if (_elapsed >= Duration)
        {
            if (Callback is not null)
            {
                Callback(this);
                Callback = null;
            }
        }
        _elapsed += time.ElapsedGameTime;
        if (_elapsed < Delay) return;

        float t = (float)Math.Min((_elapsed-Delay).TotalMilliseconds / (Duration - Delay).TotalMilliseconds, 1.0);
    
        switch (Type)
        {
            case InterpolationType.Linear:
                Position = Vector2.Lerp(From, To, t);
                break;
            case InterpolationType.Smooth:
                Position = Vector2.Lerp(From, To, t * t * (3f - 2f * t));
                break;
            case InterpolationType.EaseIn:
                Position = Vector2.Lerp(From, To, t * t);
                break;
            case InterpolationType.EaseOut:
                Position = Vector2.Lerp(From, To, t * (2f - t));
                break;
            default:
                Position = Vector2.Lerp(From, To, t);
                break;
        }

        SetPosition(Position);
    }
}