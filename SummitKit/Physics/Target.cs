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
public class Target(Vector2 to, Vector2 from, TimeSpan duration, TimeSpan delay, Action<Target> callback) : IUpdating
{
    public Vector2 To { get; init; } = to;
    public Vector2 From { get; set; } = from;
    public TimeSpan Duration { get; init; } = duration + delay;
    public TimeSpan Delay { get; init; } = delay;
    public Vector2 Position { get; private set; } = from;
    private Action<Target> _callback = callback;
    private TimeSpan _elapsed = TimeSpan.Zero;

    public bool IsComplete => _elapsed >= Duration && _callback is null;
    public void Update(GameTime time)
    {
        if (_elapsed >= Duration)
        {
            if (_callback is not null)
            {
                _callback(this);
                _callback = null;
            }
        }
        _elapsed += time.ElapsedGameTime;
        if (_elapsed < Delay) return;

        float t = (float)Math.Min((_elapsed-Delay).TotalMilliseconds / (Duration - Delay).TotalMilliseconds, 1.0);
        Position = Vector2.Lerp(From, To, t);
    }
}