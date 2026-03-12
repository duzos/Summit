using Microsoft.Xna.Framework;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

public static class PositionedExtensions
{
    public static void MoveTo(
        this IPositioned positioned,
        Vector2 to,
        TimeSpan duration,
        TimeSpan delay,
        Action<ITarget<Vector2>> callback = null,
        bool centered = true,
        bool replaceExisting = true,
        InterpolationType type = InterpolationType.Smooth)
    {
        var target = new InterpolatedTarget<Vector2>(
            to - (centered ? new Vector2(positioned.Width, positioned.Height) * 0.5f : Vector2.Zero),
            positioned.Position,
            pos => positioned.Position = pos,
            duration,
            delay,
            Vector2.Lerp,
            type,
            callback
        );

        positioned.MoveTo(target, replaceExisting);
    }
}