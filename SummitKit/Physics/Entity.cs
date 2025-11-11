using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SummitKit.Graphics;
using SummitKit.Input;
using System;

namespace SummitKit.Physics;

/// <summary>
/// Base class for all entities in the physics system.
/// </summary>
public class Entity : IDraw, IUpdating, IClickable
{
    private Sprite _sprite;
    private Rectangle _aabb;
    private Vector2 _position;
    private int _cachedWidthInt = -1;
    private int _cachedHeightInt = -1;

    public Target MoveTarget { get; set; }

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

    public bool HasCollisions { get; set; }
    public bool HasGravity { get; set; }

    public Entity(Sprite sprite)
    {
        Sprite = sprite;
        Position = Vector2.Zero;
        UpdateAABBIfNeeded();
    }

    public float Height => Sprite?.Height ?? 0f;
    public float Width => Sprite?.Width ?? 0f;
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
        if (MoveTarget is not null)
        {
            MoveTarget.Update(time);
            Position = MoveTarget.Position;

            if (MoveTarget.IsComplete)
            {
                MoveTarget = null;
            }
        }

        Sprite?.Update(time);
    }

    public virtual void Draw(SpriteBatch batch)
    {
        Sprite?.Draw(batch, Position);
    }

    public virtual void OnClick(MouseState state)
    {
        // Override in derived classes to handle clicks.
    }

    public virtual void OnRelease(MouseState state)
    {
        // Override in derived classes to handle mouse button releases.
    }

    public virtual void OnHover(MouseState state)
    {
        // Override in derived classes to handle mouse hover.
    }

    public virtual void OnDrag(MouseState state, Vector2 dragOffset)
    {
        // Override in derived classes to handle mouse drag.
    }

    public virtual void OnCollision(Entity other)
    { 
        KeepOutsideArea(other.AABB);
    }

    public void MoveTo(Vector2 pos, TimeSpan time, TimeSpan delay, Action<Target> callback = null, bool centered = true)
    {
        MoveTarget = new(centered ? pos + new Vector2(Width, Height) * 0.5F : pos, Position, time, delay, callback);
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