using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summit.Card;
using Summit.Json;
using Summit.Maths;
using SummitKit;
using SummitKit.Audio;
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

// todo seperate into multiple sub classes
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
    public float? PlayedScore { get; set; }

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

    [JsonIgnore]
    public TokenExpression.ResolveProfile ResolveProfile { get; set; } = TokenExpression.ResolveProfiles.LeftToRight;
    public GameState()
    {
        MainDeck.Shuffle();
        OnLoad();
    }

    public bool PlaySelected(bool force = false)
    {
        if ((RemainingHands <= 0 || MainHand.Selected.Count == 0) && !force) return false;

        var selectedCards = MainHand.Selected.ToImmutableList();
        foreach (var card in selectedCards)
        {
            card.SetSelected(false);
            card.Draggable = false;
            MainHand.RemoveCard(card);
            PlayedHand.AddCard(card);
            //card.ScaleTo(card.Scale * 1.1F, TimeSpan.FromSeconds(0.1F), TimeSpan.FromSeconds(0.25F));
        }
        Deal();
        DiscardDeck.AddAll(PlayedHand.Entities);
        PlayedHand.UpdatePositions(i => TimeSpan.FromSeconds(0.25F), i => TimeSpan.FromSeconds(0.1));
        RemainingHands--;

        MainHand.UpdatePositions(i => TimeSpan.FromSeconds(0.25), i => TimeSpan.FromSeconds(0.25F));
        MainHand.Draggable = false;
        TrySave();

        int index = 0;
        Scheduler.Delay(() =>
        {
            PlayedHand.Trigger(ResolveProfile, total =>
            {
                Score += total;
                PlayedScore = null;

                Scheduler.Delay(() =>
                {
                    MainHand.Draggable = true;
                    PlayedHand.DiscardAll();

                    if (!CheckGameEnd())
                    {
                        MainHand.SpawnCards();
                    }

                    TrySave();
                }, TimeSpan.FromSeconds(1));
            }, (step, card) =>
            {
                card.Trigger(index);
                PlayedScore = (float) step.After;

                card.DisplayMessage(PlayedScore.ToString());

                index++;
            });
        },TimeSpan.FromSeconds(1));

        return true;
    }


    private bool CheckGameEnd()
    {
        bool val = RemainingHands <= 0 || (MainDeck.Count == 0 && MainHand.Cards.Count == 0);

        if (val)
        {
            Scheduler.Delay(() => NextRound(), TimeSpan.FromSeconds(1));
        }

        return val;
    }
    public bool DiscardSelected(bool force = false)
    {
        if ((RemainingDiscards <= 0 || MainHand.Selected.Count == 0) && !force) return false;

        DiscardDeck.AddAll(MainHand.Selected);
        MainHand.DiscardSelected();
        Deal();
        RemainingDiscards--;
        TrySave();

        CheckGameEnd();

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

        PlayedHand.Draggable = false;
        PlayedHand.Backdrop = false;

        PlayedHand.Position = new(Core.GraphicsDevice.Viewport.Width / 2, Core.GraphicsDevice.Viewport.Height / 2 - 100);
        MainHand.Position = new Vector2(Core.GraphicsDevice.Viewport.Width / 2, Core.GraphicsDevice.Viewport.Height - 200);

        Core.Entities.Entities.Where(e => e is CardEntity)
            .ToList()
            .ForEach(Core.Entities.RemoveEntity);

        SpawnDeckTop();

        PlayedHand.Draggable = false;
        PlayedHand.Backdrop = false;

        PlayedHand.Position = new(Core.GraphicsDevice.Viewport.Width / 2, Core.GraphicsDevice.Viewport.Height / 2 - 100);
        MainHand.Position = new Vector2(Core.GraphicsDevice.Viewport.Width / 2, Core.GraphicsDevice.Viewport.Height - 200);

        // pick new song
        MusicTracker musicTracker = Core.Audio.Music;
        musicTracker.FadeInto(musicTracker.RandomSong!, TimeSpan.FromSeconds(2.5), TimeSpan.FromSeconds(2.5));

        if (CheckGameEnd()) return;

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

    private CardEntity SpawnDeckTop()
    {
        CardEntity entity = new(new(CardType.Ace, CardSuit.Spades))
        {
            Scale = new(2.5F),
            Backwards = true,
            DragReplacesExisting = true
        };
        entity.Sprite?.CenterOrigin();
        entity.HasCollisions = false;
        entity.Sprite.LayerDepth = 0.1F;
        entity.CollidesWithWindowEdges = false;
        entity.Position = new Vector2(Core.GraphicsDevice.PresentationParameters.BackBufferWidth - 110, Core.GraphicsDevice.PresentationParameters.BackBufferHeight -160);
        var pos = entity.Position;
        Core.Entities.AddEntity(entity);

        Scheduler.Repeat(task =>
        {
            if (entity.IsRemoved) task.Cancelled = true;
            if (entity.MoveTarget is VelocityTarget) return;
            // check distance
            if (entity.DistanceTo(pos) < 10F) return;

            entity.MoveTo(pos, TimeSpan.FromSeconds(0.5F), TimeSpan.Zero, replaceExisting: false, centered: false);
        }, TimeSpan.Zero, TimeSpan.FromSeconds(0.1F));

        return entity;
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