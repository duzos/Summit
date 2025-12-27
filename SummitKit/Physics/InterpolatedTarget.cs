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
public class InterpolatedTarget<T>(T to, T from, Action<T> setter, TimeSpan duration, TimeSpan delay, Func<T, T, float, T> lerp, InterpolationType type = InterpolationType.Smooth, Action<ITarget<T>> callback = null) : ITarget<T>
{
    public T To { get; init; } = to;
    public T From { get; set; } = from;
    public TimeSpan Duration { get; init; } = duration + delay;
    public TimeSpan Delay { get; init; } = delay;
    public T Position { get; protected set; } = from;
    public Action<T> SetPosition { get; set; } = setter;
    public InterpolationType Type { get; init; } = type;
    public Action<ITarget<T>> Callback { get; set; } = callback;
    public float Progress => (float)((_elapsed - Delay) / (Duration - Delay));
    private TimeSpan _elapsed = TimeSpan.Zero;
    private Func<T, T, float, T> Lerp { get; init; } = lerp;

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
                Position = Lerp(From, To, t);
                break;
            case InterpolationType.Smooth:
                Position = Lerp(From, To, t * t * (3f - 2f * t));
                break;
            case InterpolationType.EaseIn:
                Position = Lerp(From, To, t * t);
                break;
            case InterpolationType.EaseOut:
                Position = Lerp(From, To, t * (2f - t));
                break;
            default:
                Position = Lerp(From, To, t);
                break;
        }

        SetPosition(Position);
    }
}