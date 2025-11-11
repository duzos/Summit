using SummitKit;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Card;

public class Hand
{
    public int MaxSize { get; set; }
    private readonly List<CardData> _cards;
    private readonly Dictionary<CardData, CardEntity> _entities;
    private readonly ObservableCollection<CardEntity> _selected;
    private readonly HashSet<CardEntity> _selectedSet;

    // expose the observable collection so callers can add/remove items;
    // CollectionChanged is handled to animate added/removed items.
    public ObservableCollection<CardEntity> Selected => _selected;

    public Hand()
    {
        _cards = [];
        _entities = [];
        _selected = [];
        _selectedSet = [];
        _selected.CollectionChanged += Selected_CollectionChanged;
    }
    public IReadOnlyList<CardData> Cards => _cards.AsReadOnly();
    public ImmutableList<CardEntity> Entities => [.. _entities.Values];
    public bool AddCard(CardData card)
    {
        if (MaxSize > 0 && _cards.Count >= MaxSize)
            return false;

        _cards.Add(card);
        return true;
    }
    public void RemoveCard(CardData card)
    {
        _cards.Remove(card);
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

    public void SpawnCards()
    {
        if (_cards.Count == 0)
            return;

        const float spacing = 10f;

        // Ensure an entity exists for every card and add new entities to the manager,
        // but don't position them yet — we'll compute a centered layout first.
        for (int i = 0; i < _cards.Count; i++)
        {
            var card = _cards[i];

            if (!_entities.ContainsKey(card))
            {
                var entity = new CardEntity(card);
                entity.Scale *= 2;
                _entities[card] = entity;

                // place off-screen initially so MoveTo animates from somewhere visible
                entity.Position = new(Core.GraphicsDevice.Viewport.Width - entity.Width - 10, Core.GraphicsDevice.Viewport.Height - entity.Height - 10);
                Core.Entities.AddEntity(entity);
            }
        }

        // Compute total width of the hand (sum of card widths + spacing between them)
        float totalWidth = 0f;
        float[] widths = new float[_cards.Count];
        for (int i = 0; i < _cards.Count; i++)
        {
            var entity = _entities[_cards[i]];
            widths[i] = entity.Width;
            totalWidth += widths[i];
        }

        if (_cards.Count > 1)
            totalWidth += spacing * (_cards.Count - 1);

        // Center the whole hand on screen; cards are laid out left-to-right starting at startX.
        float centerX = Core.GraphicsDevice.Viewport.Width / 2f - widths[0] / 2;
        float startX = centerX - (totalWidth / 2f);
        float centerY = Core.GraphicsDevice.Viewport.Height / 2f;

        Vector2 pos = new(startX, centerY);

        for (int i = 0; i < _cards.Count; i++)
        {
            var entity = _entities[_cards[i]];
            entity.MoveTo(pos, TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.25 + (0.1 * i)));
            pos.X += widths[i] + spacing;
        }
    }

    public void DespawnCards()
    {
        foreach (var entity in _entities.Values)
        {
            MoveAndDespawn(entity, TimeSpan.FromSeconds(0.5));
        }
        _entities.Clear();
    }


    /// <summary>
    /// moves the entity off-screen and despawns it
    /// </summary>
    /// <param name="entity"></param>
    private void MoveAndDespawn(Entity entity, TimeSpan time)
    { 
        entity.MoveTo(new Vector2(Core.GraphicsDevice.Viewport.Width - entity.Width - 10, Core.GraphicsDevice.Viewport.Height - entity.Height - 10), time, TimeSpan.Zero, (t) =>
        {
            if (entity is CardEntity cardE && cardE.Data is not null)
            {
                _entities.Remove(cardE.Data);
            }
            Core.Entities.RemoveEntity(entity);
        }, false);
    }

    private void Selected_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e == null) return;

        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (CardEntity item in e.NewItems)
            {
                // only animate if it wasn't already selected
                if (_selectedSet.Add(item))
                {
                    item.MoveTo(item.Position - new Vector2(0, 10), TimeSpan.FromSeconds(0.1), TimeSpan.Zero, null, false);
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (CardEntity item in e.OldItems)
            {
                if (_selectedSet.Remove(item))
                {
                    item.MoveTo(item.Position + new Vector2(0, 10), TimeSpan.FromSeconds(0.1), TimeSpan.Zero, null, false);
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
}