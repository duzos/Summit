using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Card;

public enum CardSuit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public static class CardSuitExtensions
{
    public static string ToSymbol(this CardSuit suit) => suit switch
    {
        CardSuit.Hearts => "♥",
        CardSuit.Diamonds => "♦",
        CardSuit.Clubs => "♣",
        CardSuit.Spades => "♠",
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
    };

    public static float Apply(this CardSuit suit, ref float total, float value)
    {
        return suit switch
        {
            CardSuit.Hearts => total -= value,
            CardSuit.Diamonds => total /= value,
            CardSuit.Clubs => total += value,
            CardSuit.Spades => total *= value,
            _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
        };
    }
}