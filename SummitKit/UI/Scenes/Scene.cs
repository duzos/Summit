using Microsoft.Xna.Framework;
using SummitKit.Physics;
using SummitKit.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SummitKit.UI.Scene;

public class Scene(List<SceneData> data, TimeSpan duration)
{
    private readonly List<SceneData> _data = data;

    public IReadOnlyList<IPositioned> Children => [.. _data.Select(d => d.Positioned)];
    public IReadOnlyList<SceneData> Data => _data;

    public TimeSpan Duration { get; protected set; } = duration;

    public Scene(List<IPositioned> data, TimeSpan duration) : this([.. data.Select(e => new SceneData { Positioned = e, Position = e.Position })], duration) { }

    public virtual void Enable()
    {
        _data.ForEach(d => d.Positioned.MoveTo(d.Position, Duration, TimeSpan.Zero, centered: false));
    }

    public virtual void Disable()
    {
        // Move each positioned item towards the nearest edge of the screen
        _data.ForEach(d =>
        {
            var nearestEdge = NearestScreenEdge(d.Position, Size: new Vector2(d.Positioned.Width, d.Positioned.Height));
            d.Positioned.MoveTo(nearestEdge, Duration, TimeSpan.Zero, centered: false);
        });
    }

    public void Transition(Scene? other)
    {
        Disable();
        if (other is null) return;
        Scheduler.Delay(other.Enable, Duration);
    }

    public static Vector2 NearestScreenEdge(Vector2 position, Vector2? possibleSize = null, Vector2? Size = null)
    {
        Vector2 screenSize = possibleSize ?? Core.GraphicsDevice.Viewport.Bounds.Size.ToVector2();
        var entitySize = Size ?? Vector2.Zero;

        var left = position.X;
        var right = screenSize.X - position.X;
        var top = position.Y;
        var bottom = screenSize.Y - position.Y;
        var minDistance = Math.Min(Math.Min(left, right), Math.Min(top, bottom));
        if (minDistance == left) return new Vector2(-entitySize.X, position.Y);
        if (minDistance == right) return new Vector2(screenSize.X + entitySize.X, position.Y);
        if (minDistance == top) return new Vector2(position.X, -entitySize.Y);
        return new Vector2(position.X, screenSize.Y + entitySize.Y);
    }
}