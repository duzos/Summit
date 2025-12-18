using Microsoft.Xna.Framework;
using Summit.Card;
using SummitKit;
using SummitKit.Physics;
using SummitKit.Util;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Summit.State;

public class GameState
{
    public Deck MainDeck { get; private set; } = new();
    public Deck DiscardDeck { get; private set; } = new();
    public Hand MainHand { get; private set; } = new();
    public Hand PlayedHand { get; private set; } = new();
    public float Score { get; set; } = 0;
    public GameState()
    {
        MainDeck.Shuffle();
        PlayedHand.Draggable = false;

        MainHand.Position = new(Core.GraphicsDevice.Viewport.Width / 2, Core.GraphicsDevice.Viewport.Height / 2);
        PlayedHand.Position = new Vector2(Core.GraphicsDevice.Viewport.Width / 2, 100);
    }

    public void PlaySelected()
    {
        var selectedCards = MainHand.Selected.ToImmutableList();
        Score += MainHand.TotalValue();
        foreach (var card in selectedCards)
        {
            card.SetSelected(false);
            card.Draggable = false;
            MainHand.RemoveCard(card);
            PlayedHand.AddCard(card);
        }
        Deal();
        MainHand.SpawnCards();
        PlayedHand.UpdatePositions();
        MainHand.UpdatePositions();

        Scheduler.Delay(() =>
        {
            PlayedHand.DiscardAll();
        }, TimeSpan.FromSeconds(2));
    }

    public void ReturnDiscard()
    {
        MainDeck.AddAll(DiscardDeck.Cards);
        DiscardDeck = new();
        MainDeck.Shuffle();
    }

    public void Deal()
    {
        MainDeck.Deal(MainHand);
    }
}