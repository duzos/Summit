using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Graphics;

public class Camera : IPositioned, IUpdating
{
    public ITarget<Vector2>? MoveTarget { get; private set; }
    public ITarget<float>? ZoomTarget { get; private set; }

    public Vector2 Position { get; set; }
    public bool RoundToPixels { get; set; } = true;
    public float Width => Core.GraphicsDevice.Viewport.Width / EffectiveZoom;

    public float Height => Core.GraphicsDevice.Viewport.Height / EffectiveZoom;
    public float Zoom { get; set; } = 1f;
    public float MinZoomStep { get; set; } = 0f;
    public Rectangle? Bounds { get; set; }

    public float MinZoom
    {
        get
        {
            if (Bounds is not Rectangle b || b.Width <= 0 || b.Height <= 0) return 0f;

            var viewport = Core.GraphicsDevice.Viewport;
            float min = Math.Max((float)viewport.Width / b.Width, (float)viewport.Height / b.Height);
            return MinZoomStep > 0f ? MathF.Ceiling(min / MinZoomStep) * MinZoomStep : min;
        }
    }

    public float EffectiveZoom => Math.Max(Zoom, MinZoom);

    public Matrix GetTransform(Viewport viewport)
    {
        float zoom = EffectiveZoom;
        Vector2 pos = RoundToPixels
            ? new Vector2(MathF.Round(Position.X * zoom) / zoom, MathF.Round(Position.Y * zoom) / zoom)
            : Position;

        return Matrix.CreateTranslation(-pos.X, -pos.Y, 0f) *
               Matrix.CreateScale(zoom, zoom, 1f) *
               Matrix.CreateTranslation(MathF.Round(viewport.Width / 2f), MathF.Round(viewport.Height / 2f), 0f);
    }
    public Vector2 ScreenToWorld(Vector2 screen, Viewport viewport) =>
        Vector2.Transform(screen, Matrix.Invert(GetTransform(viewport)));

    public Rectangle VisibleArea
    {
        get
        {
            var viewport = Core.GraphicsDevice.Viewport;
            var topLeft = ScreenToWorld(Vector2.Zero, viewport);
            var bottomRight = ScreenToWorld(new Vector2(viewport.Width, viewport.Height), viewport);

            int x = (int)MathF.Floor(topLeft.X);
            int y = (int)MathF.Floor(topLeft.Y);
            return new Rectangle(x, y, (int)MathF.Ceiling(bottomRight.X) - x, (int)MathF.Ceiling(bottomRight.Y) - y);
        }
    }
    public Vector2 MouseWorld => ScreenToWorld(Core.Input.Mouse.Position.ToVector2(), Core.GraphicsDevice.Viewport);
    public void MoveTo(ITarget<Vector2> target, bool replaceExisting = true)
    {
        if (MoveTarget != null && !replaceExisting)
            return;

        MoveTarget = target;
    }

    public void ZoomTo(ITarget<float> target, bool replaceExisting = true)
    {
        if (ZoomTarget != null && !replaceExisting)
            return;

        ZoomTarget = target;
    }

    public void Follow(IPositioned target, float sharpness = 10f)
    {
        MoveTo(new FollowTarget(() => (target is Entity e) ? e.Centre : target.Position, () => Position, v => Position = v, sharpness));
    }

    public void Update(GameTime deltaTime)
    {
        MoveTarget?.Update(deltaTime);
    
        if (MoveTarget != null && MoveTarget.IsComplete)
        {
            MoveTarget.Callback?.Invoke(MoveTarget);
            MoveTarget = null;
        }
    
        ZoomTarget?.Update(deltaTime);

        if (ZoomTarget != null && ZoomTarget.IsComplete)
        {
            ZoomTarget.Callback?.Invoke(ZoomTarget);
            ZoomTarget = null;
        }

        ClampToBounds();
    }

    private void ClampToBounds()
    {
        if (Bounds is not Rectangle b) return;

        float halfW = Width / 2f;
        float halfH = Height / 2f;

        float x = Width >= b.Width ? b.Center.X : Math.Clamp(Position.X, b.Left + halfW, b.Right - halfW);
        float y = Height >= b.Height ? b.Center.Y : Math.Clamp(Position.Y, b.Top + halfH, b.Bottom - halfH);

        Position = new Vector2(x, y);
    }
}