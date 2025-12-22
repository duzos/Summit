using SummitKit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Summit.Card;

public class CardData
{
    [JsonIgnore]
    private CardType _type;
    private int _rank;
    public CardSuit Suit { get; init; }
    [JsonIgnore]
    public bool Backwards { get; set; }

    [JsonConstructor]
    public CardData(int rank, CardSuit suit, bool backwards = false)
    {
        Suit = suit;
        Rank = Math.Clamp(rank, 1, 14);
        Backwards = backwards;
    }

    public CardData(CardType type, CardSuit suit) : this(
        type switch
        {
            CardType.Ace => 1,
            CardType.Jack => 11,
            CardType.Queen => 12,
            CardType.King => 13,
            CardType.Bracket => 14,
            _ => throw new ArgumentException("Invalid rank for CardData constructor")
        },
        suit)
    {
    
    }

    public int Rank
    {
        get => _rank;
        init
        {
            _rank = value;
            _type = value switch
            {
                1 => CardType.Ace,
                11 => CardType.Jack,
                12 => CardType.Queen,
                13 => CardType.King,
                14 => CardType.Bracket,
                _ => CardType.Number
            };
        }
    }

    [JsonIgnore]
    public bool IsFaceCard => Rank > 10;
    [JsonIgnore]
    public bool IsAce => Rank == 1;
    [JsonIgnore]
    public CardType Type => _type;
    [JsonIgnore]
    public Back BackColour => Suit switch
    {
        CardSuit.Hearts => Back.Red,
        CardSuit.Diamonds => Back.Red,
        CardSuit.Clubs => Back.Blue,
        CardSuit.Spades => Back.Blue,
        _ => throw new ArgumentOutOfRangeException()
    };

    public Sprite CreateSprite(TextureAtlas atlas)
    {
        if (Backwards)
        {
            return atlas.CreateSprite(BackColour.ToString().ToLower() + "-back");
        }

        string rankString = Type switch
        {
            CardType.Number => Rank.ToString(),
            _ => Type.ToString().ToLower()
        };

        string suitString = Suit.ToString().ToLower();
        string regionName = $"{rankString}-{suitString}";
        return atlas.CreateSprite(regionName);
    }

    public string GetName()
    {
        string rankString = Type switch
        {
            CardType.Ace => "Ace",
            CardType.Jack => "Jack",
            CardType.Queen => "Queen",
            CardType.King => "King",
            _ => Rank.ToString()
        };
        return $"{rankString} of {Suit}";
    }
    public float Apply(float total)
    {
        return Suit.Apply(total, Math.Min(Rank, 10));
    }

    public enum Back
    {
        Red,
        Blue
    }
}