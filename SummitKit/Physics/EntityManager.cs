using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SummitKit.Graphics;
using SummitKit.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

public class EntityManager : IUpdating, IDraw
{
    private readonly List<Entity> _entities;
    public IReadOnlyList<Entity> Entities => _entities.AsReadOnly();
    public bool LockToScreenBounds { get; set; } = false;
    public float Gravity { get; set; } = 9.81F;
    private Entity? _draggedEntity;
    private Vector2 _dragGrabOffset;
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
            _entities.ForEach(e => e.KeepInsideArea(bounds));
        }
    }

    public void Draw(SpriteBatch sprite)
    {
        _entities.ForEach(e => e.Draw(sprite));
    }

    public void CheckClicks(MouseInfo input)
    {
        if (input.WasButtonJustPressed(MouseButton.Left))
        {
            var entity = GetEntityAtPosition(input.Position.ToVector2());
            entity?.OnClick(input.CurrentState);

            if (entity is not null)
            {
                _draggedEntity = entity;
                _dragGrabOffset = input.Position.ToVector2() - entity.Position;
            }
        }

        if (input.WasButtonJustReleased(MouseButton.Left))
        {
            var entity = _draggedEntity is not null ? _draggedEntity : GetEntityAtPosition(input.Position.ToVector2());
            entity?.OnRelease(input.CurrentState);
            if (_draggedEntity is not null)
            {
                _draggedEntity = null;
            }
        }

        var hoverEntity = GetEntityAtPosition(input.Position.ToVector2());
        hoverEntity?.OnHover(input.CurrentState);

        if (input.IsButtonDown(MouseButton.Left))
        {
            if (_draggedEntity is not null)
            {
                _draggedEntity.OnDrag(input.CurrentState, _dragGrabOffset);
            }
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

    public Entity GetEntityAtPosition(Vector2 position) => _entities.FirstOrDefault(e => e.AABB.Contains(position));
    public IEnumerable<Entity> GetEntitiesInArea(Rectangle area) => _entities.Where(e => e.AABB.Intersects(area));
}