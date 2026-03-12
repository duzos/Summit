using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

public interface IPositioned
{
    ITarget<Vector2>? MoveTarget { get; }
    Vector2 Position { get; set; }
    float Width { get; }
    float Height { get; }

    public abstract void MoveTo(ITarget<Vector2> target, bool replaceExisting = true);

    public virtual void MoveTo(Vector2 to, TimeSpan duration, TimeSpan delay, Action<ITarget<Vector2>> callback = null, bool centered = true, bool replaceExisting = true, InterpolationType type = InterpolationType.Smooth)
    {
        var target = new InterpolatedTarget<Vector2>(to - (centered ? new Vector2(Width, Height) * 0.5F : Vector2.Zero), Position, (pos) => Position = pos, duration, delay, Vector2.Lerp, type, callback);
        MoveTo(target, replaceExisting);
    }
}