using Microsoft.Xna.Framework;
using Summit.Card;
using Summit.Json;
using SummitKit;
using SummitKit.IO;
using SummitKit.Physics;
using SummitKit.Util;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Summit.State;

public class GameState : ISerializable<GameState>
{
    [JsonInclude]
    public Deck MainDeck { get; private set; } = new();
    [JsonInclude]
    public Deck DiscardDeck { get; private set; } = Deck.Empty();
    [JsonInclude]
    public Hand MainHand { get; private set; } = new();
    [JsonInclude]
    public Hand PlayedHand { get; private set; } = new();
    public float Score { get; set; } = 0;
    [JsonIgnore]
    public Random Random { get; private set; } = new();
    public int TargetScore { get; set; } = 0;
    public int RemainingHands { get; set; } = 0;
    public int RemainingDiscards { get; set; } = 0;
    [JsonIgnore]
    public Comparison<CardData> LastSort { get; set; } = Hand.SortByValue;
    [JsonIgnore]
    public string Namespace => MainGame.Name;
    [JsonIgnore]
    public string FileName => "gamestate";
    [JsonIgnore]
    public JsonSerializerOptions JsonOptions { 
        get
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            options.Converters.Add(new Vector2Converter());
            return options;
        }
    }

    public GameState()
    {
        MainDeck.Shuffle();
        PlayedHand.Draggable = false;

        MainHand.Position = new(Core.GraphicsDevice.Viewport.Width / 2, Core.GraphicsDevice.Viewport.Height / 2);
        PlayedHand.Position = new Vector2(Core.GraphicsDevice.Viewport.Width / 2, 100);
    }

    public bool PlaySelected(bool force = false)
    {
        if ((RemainingHands <= 0 || MainHand.Selected.Count <= 0) && !force) return false;

        var selectedCards = MainHand.Selected.ToImmutableList();
        foreach (var card in selectedCards)
        {
            card.SetSelected(false);
            card.Draggable = false;
            MainHand.RemoveCard(card);
            PlayedHand.AddCard(card);
        }
        Deal();
        DiscardDeck.AddAll(PlayedHand.Entities);
        PlayedHand.UpdatePositions();
        RemainingHands--;

        MainHand.UpdatePositions(i => TimeSpan.FromSeconds(0.25), i => TimeSpan.FromSeconds(0.25F));
        MainHand.Draggable = false;
        TrySave();

        Scheduler.Delay(() =>
        {
            MainHand.Draggable = true;
            Score += PlayedHand.TotalValue(false);
            PlayedHand.DiscardAll();

            if (RemainingHands <= 0)
            {
                Scheduler.Delay(() => NextRound(), TimeSpan.FromSeconds(1));
            } else
            {
                MainHand.SpawnCards();
            }

            TrySave();
        }, TimeSpan.FromSeconds(2));

        return true;
    }

    public bool DiscardSelected(bool force = false)
    {
        if ((RemainingDiscards <= 0 || MainHand.Selected.Count <= 0) && !force) return false;

        DiscardDeck.AddAll(MainHand.Selected);
        MainHand.DiscardSelected();
        Deal();
        RemainingDiscards--;
        TrySave();

        return true;
    }
    private void ReturnDiscard()
    {
        MainDeck.AddAll(DiscardDeck.Cards);
        DiscardDeck = Deck.Empty();
        MainDeck.Shuffle();
    }

    public void Deal()
    {
        MainDeck.Deal(MainHand);
        MainHand.SpawnCards();
        TrySave();
    }

    private void RandomiseTargetScore(int min = 1, int max = 1001)
    {
        TargetScore = Random.Next(min, max);
    }

    public void NextRound(int hands = 5, int discards = 3, int minScore = 1, int maxScore = 1001, int? targetScore = null)
    {
        if (targetScore == null) {
            RandomiseTargetScore(minScore, maxScore);
        } else {
            TargetScore = targetScore.Value;
        }

        Score = 0;
        RemainingHands = hands;
        RemainingDiscards = discards;

        MainDeck.AddAll(MainHand.Cards);
        MainHand.DiscardAll();
        ReturnDiscard();

        Scheduler.Delay(Deal, TimeSpan.FromSeconds(1));

        TrySave();
    }

    public void OnLoad()
    {
        // kill all old card entities
        
        Core.Entities.Entities.Where(e => e is CardEntity)
            .ToList()
            .ForEach(Core.Entities.RemoveEntity);

        if (RemainingHands <= 0)
        {
            NextRound();
            return;
        }

        Deal();
        PlayedHand.SpawnCards();

        Scheduler.Delay(() =>
        {
            if (PlayedHand.Cards.Count != 0)
            {
                Score += PlayedHand.TotalValue(false);
            }

            PlayedHand.DiscardAll();
        }, TimeSpan.FromSeconds(2));
    }

    private void TrySave()
    {
        try
        {
            ((ISerializable<GameState>)this).Save();
        } catch (Exception e)
        {
            Core.Console.Context.Error("Failed to save game state" + e.Message);
        }
    }
}