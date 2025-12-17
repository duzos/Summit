using Microsoft.Xna.Framework;
using Summit.Card;
using SummitKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Summit.State;

public class GameState
{
    public Deck MainDeck { get; private set; } = new();
    public Hand MainHand { get; private set; } = new();
    public Hand PlayedHand { get; private set; } = new();

    public GameState()
    {
        MainDeck.Shuffle();
        PlayedHand.Draggable = false;

        MainHand.Position = new(Core.GraphicsDevice.Viewport.Width / 2, Core.GraphicsDevice.Viewport.Height / 2);
        PlayedHand.Position = new Vector2(0,0);
    }

    public void PlaySelected()
    {
        var selectedCards = MainHand.Selected.ToList();
        foreach (var card in selectedCards)
        {
            MainHand.RemoveCard(card);
            PlayedHand.AddCard(card);
            card.SetSelected(false);
            card.Draggable = false;
        }
        MainHand.UpdatePositions();
        PlayedHand.UpdatePositions();
    }

    public void Deal()
    {
        MainDeck.Deal(MainHand);
    }
}