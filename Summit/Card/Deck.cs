using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Card;

public class Deck(IEnumerable<CardData> cards)
{
    public Queue<CardData> Cards { get; init; } = new Queue<CardData>(cards);

    public int Count => Cards.Count;

    /// <summary>
    /// Creates a deck with every card in a standard 52-card deck.
    /// </summary>
    public Deck() : this(
        from suit in Enum.GetValues<CardSuit>()
        from rank in Enumerable.Range(1, 13)
        select new CardData(rank, suit))
    {

    }

    public void Shuffle(Random? rng = null)
    {
        rng ??= new Random();
        var cardList = Cards.ToList();
        int n = cardList.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            (cardList[n], cardList[k]) = (cardList[k], cardList[n]);
        }
        Cards.Clear();
        foreach (var card in cardList)
        {
            Cards.Enqueue(card);
        }
    }

    public void AddAll(IEnumerable<CardData> cards)
    {
        foreach (var card in cards)
        {
            Cards.Enqueue(card);
        }
    }

    public void AddAll(IEnumerable<CardEntity> cards)
    {
        foreach (var card in cards)
        {
            if (card == null || card.Data == null) continue;
            Cards.Enqueue(card.Data);
        }
    }
    public void Deal(Hand hand)
    {
        if (Cards.Count == 0) return;
        
        while (!hand.IsFull() && Cards.Count > 0)
        {
            var card = Cards.Dequeue();
            hand.AddCard(card);
        }
    }
}