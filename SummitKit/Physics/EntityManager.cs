using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SummitKit.Graphics;
using SummitKit.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

public class EntityManager : IUpdating, IDraw
{
    private readonly List<Entity> _entities;
    public IReadOnlyList<Entity> Entities => _entities.AsReadOnly();
    public bool LockToScreenBounds { get; set; } = true;
    public float Gravity { get; set; } = 9.81F;
    private DragHandler _drag = new();
    public Entity? DraggedEntity => _drag.Dragged;
    public Entity? HoveredEntity { get; private set; }
    private Entity? _lastClick;

    public float LayerDepth { get; set; } = 0.0F;
    public EntityManager()
    {
        _entities = [];
    }
    public void AddEntity(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _entities.Add(entity);
    }
    public void RemoveEntity(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _entities.Remove(entity);
    }
    public void ClearEntities()
    {
        _entities.Clear();
    }

    public void Update(GameTime gameTime)
    {
        // todo ToList is a costly solution to concurrent modification
        _entities.ToList().ForEach(e => e.Update(gameTime));
        PerformCollisions();

        if (LockToScreenBounds)
        {
            var bounds = new Rectangle(0, 0, Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height);
            _entities.ForEach(e => { if (e.CollidesWithWindowEdges) e.KeepInsideArea(bounds); });
        }

        _drag.Update(gameTime);
    }

    public void Draw(SpriteBatch sprite)
    {
        _entities.ForEach(e => e.Draw(sprite));
    }

    public void CheckClicks(MouseInfo input)
    {
        if (input.WasButtonJustPressed(MouseButton.Left))
        {
            var entity = GetEntityAtPosition(input.Position.ToVector2(), e => e.Clickable);
            entity?.OnClick(input.CurrentState);
            _lastClick = entity;
        }

        if (input.WasButtonJustReleased(MouseButton.Left))
        {
            var entity = DraggedEntity is not null ? DraggedEntity : (GetEntityAtPosition(input.Position.ToVector2(), e => e.Clickable));
            bool dragged = DraggedEntity is not null;
            _drag.Release();
            entity?.OnRelease(input.CurrentState, dragged);
            
            if (_lastClick is not null)
            {
                _lastClick.OnRelease(input.CurrentState, dragged);
                _lastClick = null;
            }
        }

        var preHover = HoveredEntity;
        HoveredEntity = GetEntityAtPosition(input.Position.ToVector2(), e => e.Clickable);
        if (preHover != HoveredEntity)
        {
            preHover?.OnHoverStop(input.CurrentState);
        }
        HoveredEntity?.OnHover(input.CurrentState);

        if (input.IsButtonDown(MouseButton.Left))
        {
            if (HoveredEntity is not null && HoveredEntity.Draggable)
            {
                _drag.Possible = HoveredEntity;
                _drag.PossibleOffset = input.Position.ToVector2() - HoveredEntity.Position;
            }

            DraggedEntity?.OnDrag(input.CurrentState, _drag.DragOffset);
        }
    }

    private void PerformCollisions()
    {
        // get all entities which have collisions enabled
        var list = _entities.Where(e => e.HasCollisions).ToList();

        // todo this will be bad at large scale.
        for (int i = 0; i < list.Count; i++)
        {
            // apply gravity
            var entityA = list[i];
            if (Gravity != 0F && entityA.HasGravity)
            {
                entityA.Move(new Vector2(0F, Gravity));
            }

            for (int j = i + 1; j < list.Count; j++)
            {
                var entityB = list[j];
                if (entityA.AABB.Intersects(entityB.AABB))
                {
                    entityA.OnCollision(entityB);
                    entityB.OnCollision(entityA);
                }
            }
        }
    }

    public Entity GetEntityAtPosition(Vector2 position, Predicate<Entity>? filter = null)
    {
        Entity? best = null;
        float bestDepth = float.NegativeInfinity;
        var list = filter is null ? _entities : _entities.Where(filter.Invoke);

        foreach (var e in list)
        {
            if (!e.AABB.Contains(position))
                continue;

            if (best == null || e.LayerDepth > bestDepth)
            {
                best = e;
                bestDepth = e.LayerDepth;
            }
        }

        return best;
    }
    public IEnumerable<Entity> GetEntitiesInArea(Rectangle area) => _entities.Where(e => e.AABB.Intersects(area));

    public Entity GetNearestEntity(Vector2 position, float maxDistance, Predicate<Entity> predicate)
    {
        Entity? nearest = null;
        float nearestDistSq = maxDistance * maxDistance;
        foreach (var entity in _entities)
        {
            float distSq = Vector2.DistanceSquared(entity.Position, position);
            if (distSq < nearestDistSq && predicate.Invoke(entity))
            {
                nearest = entity;
                nearestDistSq = distSq;
            }
        }
        return nearest;
    }
}