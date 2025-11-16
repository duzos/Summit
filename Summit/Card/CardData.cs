using SummitKit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Card;

public class CardData
{
    private CardType _type;
    private int _rank;
    public CardSuit Suit { get; init; }
    public bool Backwards { get; set; }

    public CardData(int rank, CardSuit suit)
    {
        Suit = suit;
        Rank = Math.Clamp(rank, 1, 13);
    }

    public CardData(CardType type, CardSuit suit) : this(
        type switch
        {
            CardType.Ace => 1,
            CardType.Jack => 11,
            CardType.Queen => 12,
            CardType.King => 13,
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
                _ => CardType.Number
            };
        }
    }

    public bool IsFaceCard => Rank > 10;
    public bool IsAce => Rank == 1;

    public CardType Type => _type;

    public Sprite CreateSprite(TextureAtlas atlas)
    {
        if (Backwards)
        {
            return atlas.CreateSprite("blue-back");
        }

        string rankString = Type switch
        {
            CardType.Ace => "ace",
            CardType.Jack => "jack",
            CardType.Queen => "queen",
            CardType.King => "king",
            _ => Rank.ToString()
        };

        string suitString = Suit.ToString().ToLower();
        string regionName = $"{rankString}-{suitString}";
        return atlas.CreateSprite(regionName);
    }

    public float Apply(float total)
    {
        return Suit.Apply(total, Math.Min(Rank, 10));
    }
}