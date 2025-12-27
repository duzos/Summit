using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summit.Maths;
using SummitKit;
using SummitKit.Graphics;
using SummitKit.Physics;
using SummitKit.UI;
using SummitKit.Util;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Summit.Card;

public class Hand : IPositioned, IDraggable, IDraw
{
    public static readonly Comparison<CardData> SortByValue = (a, b) =>
    {
        int aValue = a.Rank == 1 ? 14 : a.Rank;
        int bValue = b.Rank == 1 ? 14 : b.Rank;
        return aValue != bValue ? bValue - aValue : a.Suit.CompareTo(b.Suit);
    };

    public static readonly Comparison<CardData> SortBySuit = (a, b) =>
    {
        int suitComparison = a.Suit.CompareTo(b.Suit);
        if (suitComparison != 0)
            return suitComparison;
        int aValue = a.Rank == 1 ? 14 : a.Rank;
        int bValue = b.Rank == 1 ? 14 : b.Rank;
        return bValue - aValue;
    };

    public int SelectedMaxSize { get; set; } = 5;
    public int MaxSize { get; set; } = 8;
    [JsonInclude]
    private List<CardData> _cards { get; set; }
    [JsonIgnore]
    private readonly Dictionary<CardData, CardEntity> _entities;
    [JsonIgnore]
    private readonly ObservableCollection<CardEntity> _selected;
    [JsonIgnore]
    private readonly HashSet<CardEntity> _selectedSet;
    private Vector2 _centrePos;
    public float Width { get; set; } = 512;
    public float Height { get; private set; } = 50;
    public float LayerDepth { get; set; } = 0.01F;
    public Rectangle AABB { get; private set; }
    private Texture2D _pixel;
    public float Padding { get; set; } = 8;
    public Vector2 Position
    {
        get => _centrePos;
        set
        {
            _centrePos = value;
            UpdatePositions();
        }
    }

    public bool Draggable
    {
        get => _entities.Values.All(e => e.Draggable);
        set
        {
            foreach (var entity in _entities.Values)
                entity.Draggable = value;
        }
    }
    [JsonIgnore]
    public ObservableCollection<CardEntity> Selected => _selected;

    public Hand()
    {
        _cards = [];
        _entities = [];
        _selected = [];
        _selectedSet = [];
        _selected.CollectionChanged += Selected_CollectionChanged;
        _pixel = UIContainer.CreateRoundedRectangle(Core.Graphics.GraphicsDevice, (int)Width, (int)Height, 8, Color.White);
    }
    [JsonIgnore]
    public IReadOnlyList<CardData> Cards => _cards.AsReadOnly();
    [JsonIgnore]
    public ImmutableList<CardEntity> Entities => [.. _entities.Values];

    public bool AddCard(CardEntity card)
    {
        if (!AddCard(card.Data))
            return false;

        card.ParentHand = this;
        _entities[card.Data] = card;
        return true;
    }

    public void RemoveCard(CardEntity card)
    {
        card.ParentHand = null;
        RemoveCard(card.Data);
    }

    public bool AddCard(CardData card, bool force = false)
    {
        if (MaxSize > 0 && _cards.Count >= MaxSize && !force)
            return false;

        _cards.Add(card);
        return true;
    }
    public void RemoveCard(CardData card)
    {
        if (_entities[card] is not null)
        {
            _selected.Remove(_entities[card]);
        }

        _cards.Remove(card);
        _entities.Remove(card);
    }
    public void Clear()
    {
        _selected.Clear();
        _cards.Clear();
    }

    public bool IsFull()
    {
        if (MaxSize <= 0)
            return false;
        return _cards.Count >= MaxSize;
    }

    public bool IsSelectedFull()
    {
        if (SelectedMaxSize <= 0)
            return false;
        return _selected.Count >= SelectedMaxSize;
    }

    public float TotalValue(bool selectedOnly = true)
    {
        float total = 0f;

        List<CardEntity> list = (selectedOnly ? Selected.ToList() : _entities.Values.ToList());
        foreach (var cardEntity in list)
        {
            cardEntity.Data.Apply(ref total);
        }

        return total;
    }

    public void Trigger(TokenExpression.ResolveProfile profile, Action<float>? finished = null, Action<TokenExpression.ApplyStep, CardEntity>? update = null, bool selectedOnly = false)
    {
        List<CardEntity> list = (selectedOnly ? [.. Selected] : _entities.Values.ToList());
        List<TokenExpression.ApplyStep> steps = TokenExpression.BuildApplySteps(this, profile, selected: selectedOnly);

        int i = 0;
        foreach (var step in steps)
        {
            Scheduler.Delay(() =>
            {

                update?.Invoke(step, list.ElementAt(step.CardIndex));
            }, TimeSpan.FromSeconds(i * 1F));
            i++;
        }

        Scheduler.Delay(() =>
        {
            finished?.Invoke((float)steps.Last().After);
        }, TimeSpan.FromSeconds(i * 1F + 0.5F));
    }

    public void SpawnCards()
    {
        if (_cards.Count == 0)
            return;

        // Ensure an entity exists for every card and add new entities to the manager,
        // but don't position them yet — we'll compute a centered layout first.
        for (int i = 0; i < _cards.Count; i++)
        {
            var card = _cards[i];

            if (!_entities.ContainsKey(card))
            {
                var entity = new CardEntity(card)
                {
                    ParentHand = this
                };
                entity.Scale *= 2.5F;
                _entities[card] = entity;

                // place off-screen initially so MoveTo animates from somewhere visible
                entity.Sprite?.CenterOrigin();
                entity.CollidesWithWindowEdges = false;
                entity.HasCollisions = false;
                entity.Position = new(Core.GraphicsDevice.Viewport.Width + entity.Width, Core.GraphicsDevice.Viewport.Height - entity.Height);
                entity.SetSelected(false);
                entity.Backwards = true;
                entity.Flip(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(0.3), TimeSpan.FromSeconds(0.25));

                Core.Entities.AddEntity(entity);
            }
        }

        SortCards(MainGame.State.LastSort);
        //UpdatePositions(i => TimeSpan.FromSeconds(0.1 + (0.05 * i)), i => TimeSpan.Zero);
    }


    public void SortCards(Comparison<CardData> comparison, bool delay = false)
    {
        _cards.Sort(comparison);
        UpdatePositions(i => TimeSpan.FromSeconds(0.25/* + (0.05 * i)*/), i => (delay ? TimeSpan.FromSeconds(0.15 * i) : TimeSpan.Zero));
    }

    public void UpdatePositions(Func<int, TimeSpan> indexToSpeed, Func<int, TimeSpan> indexToDelay, Action<CardEntity>? moveCallback = null)
    {
        int count = _entities.Count;
        if (count == 0)
            return;

        var entities = _entities.Values;

        if (count == 1)
        {
            if (!entities.First().IsBeingDragged)
            {
                entities.First().MoveTo(new(Position.X - entities.First().Width / 2, Position.Y), indexToSpeed.Invoke(0), indexToDelay.Invoke(0), callback: target => { moveCallback?.Invoke(entities.First()); }, replaceExisting: false);
            }

            return;
        }

        float totalItemWidth = entities.Sum(i => i.Width);

        // Space left for gaps
        float availableSpace = Width - totalItemWidth;

        // Positive = spacing, Negative = overlap
        float gap = availableSpace / (count - 1);

        // Total width including gaps
        float layoutWidth = totalItemWidth + gap * (count - 1);

        // Left edge of the whole layout
        float leftEdge = Position.X - layoutWidth / 2f;

        float x = leftEdge;

        int j = 0;
        for (int i = 0; i < Cards.Count; i++)
        {
            if (!_entities.ContainsKey(_cards[i]))
                continue;
            j++;
            var entity = _entities[_cards[i]];

            // Convert left-edge placement to centre placement
            if (!entity.IsBeingDragged)
            {
                var entityCopy = entity;
                    entity.MoveTo(new(x - entity.Width / 2, Position.Y), indexToSpeed.Invoke(j), indexToDelay.Invoke(j), callback: target => { moveCallback?.Invoke(entityCopy); }, replaceExisting: false);
            }

            if (entity.Sprite != null)
            {
                entity.Sprite.LayerDepth = 0.1F + ((float)j) / (_cards.Count + 1);
            }
            x += entity.Width + gap;
        }

        RecalculateAABB();
    }

    public void UpdatePositions()
    {
        UpdatePositions(i => TimeSpan.FromSeconds(0.1), i => TimeSpan.Zero);
    }

    public void UpdateIndex(CardEntity entity)
    {
        if (!_entities.ContainsValue(entity))
            return;

        CardEntity nearest = Core.Entities.GetNearestEntity(Core.Input.Mouse.Position.ToVector2(), float.MaxValue, val => val != entity && val is CardEntity valCard && _entities.ContainsValue(valCard)) as CardEntity;
       
        int curIndex = _cards.IndexOf(entity.Data);
        int targetIndex = nearest != null ? _cards.IndexOf(nearest.Data) : curIndex;

        // insert this card at the target index
        if (curIndex != targetIndex && curIndex != targetIndex - 1) // todo fix last index issue
        {
            _cards.RemoveAt(curIndex);
            _cards.Insert(targetIndex, entity.Data);
            UpdatePositions();
        }
    }

    public void DespawnCards()
    {
        foreach (var entity in _entities.Values)
        {
            MoveAndDespawn(entity, TimeSpan.FromSeconds(0.5), TimeSpan.Zero);
        }
        _entities.Clear();
    }

    public void DiscardSelected()
    {
        Discard(_selected);

        _selected.Clear();
    }

    public void DiscardAll()
    {
        Discard(_entities.Values);
        _cards.Clear();
        _selected.Clear();
    }

    public void Discard(IEnumerable<CardEntity> cards)
    {
        int count = 0;
        // sort by their distance from the right side of the screen, closest 
        var entitiesByRightDistance = cards
            .OrderBy(e => Core.GraphicsDevice.Viewport.Width - (e.Position.X + e.Width / 2))
            .ToList();
        foreach (var entity in entitiesByRightDistance)
        {
            _cards.Remove(entity.Data);

            count++;
            MoveAndDespawn(entity, TimeSpan.FromSeconds(0.75F), TimeSpan.FromSeconds(count * 0.05F));
        }
    }

    /// <summary>
    /// moves the entity off-screen and despawns it
    /// </summary>
    /// <param name="entity"></param>
    public void MoveAndDespawn(Entity entity, TimeSpan time, TimeSpan delay)
    {
        if (entity is CardEntity cardE && cardE.Data is not null)
        {
            _entities.Remove(cardE.Data);
            cardE.Flip(TimeSpan.FromSeconds(0.1), TimeSpan.Zero, TimeSpan.FromSeconds(0.1));
        }

        // corner position off-screen \/
        // new Vector2(Core.GraphicsDevice.Viewport.Width - entity.Width - 10, Core.GraphicsDevice.Viewport.Height - entity.Height - 10)
        entity.CollidesWithWindowEdges = false;
        entity.MoveTo(new(Core.GraphicsDevice.Viewport.Width + entity.Width, entity.Position.Y), time, delay, (t) =>
        {
            entity.Remove();
        }, false);
    }

    private void Selected_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e == null) return;

        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            // When items are added, enforce SelectedMaxSize. If the addition would exceed
            // the limit, reject those items by removing them from the collection.
            foreach (CardEntity item in e.NewItems)
            {
                if (_selectedSet.Contains(item))
                    continue;

                if (SelectedMaxSize > 0 && _selected.Count > SelectedMaxSize)
                {
                    _selected.Remove(item);
                }
                else
                {
                    _selectedSet.Add(item);
                    item.MoveTo(item.Position - new Vector2(0, 10), TimeSpan.FromSeconds(0.1), TimeSpan.Zero, null, false, false);
                }
            }
            ReorderSelectedToMatchCards();
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (CardEntity item in e.OldItems)
            {
                if (_selectedSet.Remove(item))
                {
                    item.MoveTo(item.Position + new Vector2(0, 10), TimeSpan.FromSeconds(0.1), TimeSpan.Zero, null, false, false);
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // collection was cleared (Reset). Move all previously selected items back.
            foreach (var item in _selectedSet.ToArray())
            {
                item.MoveTo(item.Position + new Vector2(0, 10), TimeSpan.FromSeconds(0.1), TimeSpan.Zero, null, false);
            }
            _selectedSet.Clear();
        }
    }

    private void ReorderSelectedToMatchCards()
    {
        if (_selected.Count <= 1) return;

        var ordered = _selected
                       .OrderBy(ent =>
                       {
                           int idx = _cards.IndexOf(ent.Data);
                           return idx < 0 ? int.MaxValue / 2 : idx;
                       })
                       .ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            var desired = ordered[i];
            int currentIndex = _selected.IndexOf(desired);
            if (currentIndex != i)
            {
                _selected.Move(currentIndex, i);
            }
        }
    }

    public void RecalculateAABB()
    {
        Height = _entities.Values.Max(e => e.Height);
        AABB = new(
            (int)(Position.X - Width / 2 - Padding / 2),
            (int)(Position.Y + Height / 2 + Padding / 2),
            (int)(Width + Padding),
            (int)(Height + Padding)
        );

        _pixel = UIContainer.CreateRoundedRectangle(Core.Graphics.GraphicsDevice, (int)Width, (int)Height, 8, Color.White);
    }

    public void Draw(SpriteBatch batch)
    {
        // draw a grey translucent rectangle behind the hand
        batch.Draw(_pixel, AABB, null, Color.Gray * 0.5F, 0F, Vector2.Zero, SpriteEffects.None, LayerDepth);
    }
}