using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SummitKit.Graphics;
using SummitKit.Input;
using System;
using System.Formats.Tar;

namespace SummitKit.Physics;

/// <summary>
/// Base class for all entities in the physics system.
/// </summary>
public class Entity : IDraw, IUpdating, IClickable, IDraggable, IPositioned
{
    private Sprite _sprite;
    private Rectangle _aabb;
    private Vector2 _position;
    private int _cachedWidthInt = -1;
    private int _cachedHeightInt = -1;

    public ITarget<Vector2> MoveTarget { get; protected set; }
    private ITarget<Vector2> _queued;
    public ITarget<Vector2> ScaleTarget { get; protected set; }
    public Vector2 Velocity { get; set; }
    public Vector2 Friction { get; set; } = new Vector2(0.98f, 0.98f);
    public Sprite Sprite
    {
        get => _sprite;
        protected set
        {
            if (!ReferenceEquals(_sprite, value))
            {
                _sprite = value;
                InvalidateAABB();
            }
        }
    }

    /// <summary>
    /// Axis-aligned bounding box.
    /// </summary>
    public Rectangle AABB
    {
        get
        {
            UpdateAABBIfNeeded();
            return _aabb;
        }
        private set => _aabb = value;
    }

    public Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            AABB = new Rectangle((int)_position.X, (int)_position.Y, AABB.Width, AABB.Height);
        }
    }

    public Vector2 Centre
    {
        get
        {
            UpdateAABBIfNeeded();
            return new Vector2(AABB.X + AABB.Width / 2f, AABB.Y + AABB.Height / 2f);
        }
    }

    public bool HasCollisions { get; set; }
    public bool HasGravity { get; set; }
    public bool CollidesWithWindowEdges { get; set; } = true;
    public bool Draggable { get; set; } = true;
    public bool DragFollowsCursor { get; set; } = true;
    public Shadow Shadow => Sprite?.Shadow;
    public float Rotation 
    {
        get => Sprite?.Rotation ?? 0f;
        set
        {
            if (Sprite != null)
            {
                Sprite.Rotation = value;
            }
        }
    }
    public Entity(Sprite sprite)
    {
        Sprite = sprite;
        Position = Vector2.Zero;
        UpdateAABBIfNeeded();
    }

    public float Height => Sprite?.Height ?? 0f;
    public float Width => Sprite?.Width ?? 0f;
    public bool IsHovered => AABB.Contains(Core.Input.Mouse.Position);
    public Vector2 Scale {
        get => Sprite?.Scale ?? Vector2.Zero;
        set
        {
            if (Sprite != null)
            {
                Sprite.Scale = value;
                InvalidateAABB();
            }
        }
    }

    public bool Intersects(Rectangle other)
    {
        return AABB.Intersects(other);
    }

    public bool Within(Rectangle other)
    {
        return other.Contains(AABB);
    }

    public bool IsBeingDragged => Core.Entities.DraggedEntity == this;

    /// <summary>
    /// Ensure this entity's position is adjusted so that its AABB is fully inside <paramref name="container"/>.
    /// If the AABB is already within the container nothing changes.
    /// If the entity is larger than the container on an axis, it will align to the container's top/left on that axis.
    /// </summary>
    public void KeepInsideArea(Rectangle container)
    {
        if (Within(container))
            return;

        UpdateAABBIfNeeded();

        float x = Position.X;
        float y = Position.Y;
        float minX = container.Left;
        float maxX = container.Right - AABB.Width;
        float minY = container.Top;
        float maxY = container.Bottom - AABB.Height;

        if (maxX < minX) maxX = minX;
        if (maxY < minY) maxY = minY;

        if (x < minX) x = minX;
        if (x > maxX) x = maxX;
        if (y < minY) y = minY;
        if (y > maxY) y = maxY;

        Position = new Vector2(x, y);
    }

    /// <summary>
    /// If this entity intersects <paramref name="obstacle"/>, move the entity the minimal distance
    /// along one axis so that the AABB no longer intersects the obstacle.
    /// If there is no intersection nothing changes.
    /// </summary>
    public void KeepOutsideArea(Rectangle obstacle)
    {
        if (!Intersects(obstacle))
            return;

        UpdateAABBIfNeeded();


        float moveLeft = AABB.Right - obstacle.Left;      
        float moveRight = obstacle.Right - AABB.Left;     
        float moveUp = AABB.Bottom - obstacle.Top;        
        float moveDown = obstacle.Bottom - AABB.Top;     

        (float amount, Action apply)[] options =
        [
            (moveLeft > 0 ? moveLeft : float.PositiveInfinity, () => Position = new Vector2(Position.X - moveLeft, Position.Y)),
            (moveRight > 0 ? moveRight : float.PositiveInfinity, () => Position = new Vector2(Position.X + moveRight, Position.Y)),
            (moveUp > 0 ? moveUp : float.PositiveInfinity, () => Position = new Vector2(Position.X, Position.Y - moveUp)),
            (moveDown > 0 ? moveDown : float.PositiveInfinity, () => Position = new Vector2(Position.X, Position.Y + moveDown))
        ];

        float best = float.PositiveInfinity;
        int bestIndex = -1;
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i].amount < best)
            {
                best = options[i].amount;
                bestIndex = i;
            }
        }

        if (bestIndex >= 0 && !float.IsPositiveInfinity(best))
        {
            options[bestIndex].apply();
        }
    }

    public void Move(Vector2 delta)
    {
        Position += delta;
    }

    public void CentreOn(Vector2 point)
    {
        UpdateAABBIfNeeded();
        Position = new Vector2(point.X - AABB.Width / 2f, point.Y - AABB.Height / 2f);
    }

    public virtual void Update(GameTime time)
    {
        // Use frame delta
        float dt = (float)time.ElapsedGameTime.TotalSeconds;
        if (dt <= 0f) dt = 1f / 60f;

        // Apply velocity (frame-rate independent)
        Position += Velocity * dt;

        // Apply multiplicative friction as a per-second multiplier converted to a per-frame multiplier.
        // This avoids the very-large-per-frame damping that causes jitter/oscillation.
        Velocity *= new Vector2(MathF.Pow(Friction.X, dt), MathF.Pow(Friction.Y, dt));

        if (MoveTarget is not null)
        {   
            MoveTarget.Update(time);
            //Position = MoveTarget.Position;


            const float snapDistance = 1f;
            Vector2 to = MoveTarget.To;
            if (Vector2.Distance(Position, to) < snapDistance)
            {
                Position = to;
                Velocity = Vector2.Zero;
            }

            if (MoveTarget.IsComplete)
            {
                MoveTarget = null;
            }
        } else if (_queued is not null)
        {
            _queued.From = Position;
            MoveTarget = _queued;
            _queued = null;
        }

        if (ScaleTarget is not null)
        {
            ScaleTarget.Update(time);

            if (ScaleTarget.IsComplete)
            {
                ScaleTarget = null;
            }
        }

        Sprite?.Update(time);
    }

    public virtual void Draw(SpriteBatch batch)
    {
        Sprite?.Draw(batch, (Vector2)(Position + Sprite?.Origin * 2));
    }

    public virtual void OnClick(MouseState state)
    {
        // Override in derived classes to handle clicks.
    }

    public virtual void OnRelease(MouseState state, bool wasBeingDragged)
    {
        // Override in derived classes to handle mouse button releases.
    }

    public virtual void OnHover(MouseState state)
    {
        // Override in derived classes to handle mouse hover.
    }


    public virtual void OnDrag(MouseState state, Vector2 dragOffset)
    {
        if (Draggable && DragFollowsCursor && MoveTarget is null)
        {
            MoveTo(new VelocityTarget(() => Core.Input.Mouse.CurrentState.Position.ToVector2() - dragOffset, this, null, () => IsBeingDragged, 10, 50, Width), false);
        }
    }

    public virtual void OnCollision(Entity other)
    { 
        KeepOutsideArea(other.AABB);
    }

    public void MoveTo(ITarget<Vector2> target, bool replaceExisting = true)
    {
        if (MoveTarget is not null)
        {
            if (replaceExisting && MoveTarget.To == target.To) return;
            else if (!replaceExisting)
            {
                _queued = target;
            }
            return;
        }

        MoveTarget = target;
    }

    public void MoveTo(Vector2 to, TimeSpan duration, TimeSpan delay, Action<ITarget<Vector2>> callback = null, bool centered = true, bool replaceExisting = true, InterpolationType type = InterpolationType.Smooth)
    {
        var target = new InterpolatedTarget<Vector2>(to + (centered ? new Vector2(Width, Height) * 0.5F : Vector2.Zero), Position, (pos) => Position = pos, duration, delay, Vector2.Lerp, type, callback);
        MoveTo(target, replaceExisting);
    }

    public void ScaleTo(ITarget<Vector2> target, bool replaceExisting = true)
    {
        if (ScaleTarget is not null && !replaceExisting) return;

        ScaleTarget = target;
    }

    public void ScaleTo(Vector2 to, TimeSpan duration, TimeSpan delay, Action<ITarget<Vector2>> callback = null, bool replaceExisting = true, InterpolationType type = InterpolationType.Smooth)
    {
        var target = new InterpolatedTarget<Vector2>(to, Scale, (scale) => Scale = scale, duration, delay, Vector2.Lerp, type, callback);
        ScaleTo(target, replaceExisting);
    }

    /// <summary>
    /// Force AABB to be recalculated on next access.
    /// Call this if the Sprite mutates (scale/origin/etc.) but the Sprite instance stays the same.
    /// </summary>
    public void InvalidateAABB()
    {
        _cachedWidthInt = -1;
        _cachedHeightInt = -1;
    }

    private void UpdateAABBIfNeeded()
    {
        int curW = (int)Math.Round(Width);
        int curH = (int)Math.Round(Height);

        if (curW != _cachedWidthInt || curH != _cachedHeightInt)
        {
            _aabb = new Rectangle(_aabb.X, _aabb.Y, curW, curH);

            _cachedWidthInt = curW;
            _cachedHeightInt = curH;
        }
    }
}