using Summit.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Summit.Maths;

public static class TokenExpression {
    public enum Op
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    public abstract record Token;

    public record ValueToken(double Value) : Token;

    public record OperatorToken(Op Operator) : Token;

    public record ResolveGroup(params Op[] Operators);

    public sealed class ResolveProfile(string name, params TokenExpression.ResolveGroup[] groups)
    {
        public string Name { get; } = name;
        public IReadOnlyList<ResolveGroup> Groups { get; } = groups;
    }

    public static class ResolveProfiles
    {
        public static readonly ResolveProfile LeftToRight =
            new("Left-to-Right",
                new ResolveGroup(Op.Add, Op.Subtract, Op.Multiply, Op.Divide)
            );

        public static readonly ResolveProfile BIDMAS =
            new("BIDMAS",
                new ResolveGroup(Op.Multiply, Op.Divide),
                new ResolveGroup(Op.Add, Op.Subtract)
            );

        public static readonly ResolveProfile Inverted =
            new("Inverted",
                new ResolveGroup(Op.Add, Op.Subtract),              
                new ResolveGroup(Op.Multiply, Op.Divide)
            );
    }

    public record ApplyStep(
        int CardIndex,
        Op Operator,
        int Value,
        double Before,
        double After
    );

    public static List<ApplyStep> BuildApplySteps(
        double startValue,
        List<CardData> cards,
        ResolveProfile profile)
    {
        double total = startValue;
        var steps = new List<ApplyStep>();
        var remaining = cards
            .Select((c, i) => (Card: c, Index: i))
            .ToList();

        foreach (var group in profile.Groups)
        {
            foreach (var entry in remaining.Where(e => group.Operators.Contains(e.Card.Suit.ToOperation())))
            {
                Op op = entry.Card.Suit.ToOperation();
                double before = total;
                total = Apply(total, entry.Card.Rank, op);
                if (total == 0 && (op == Op.Multiply || op == Op.Divide))
                {
                    total = Apply(total, entry.Card.Rank, Op.Add);
                }
                steps.Add(new ApplyStep(
                    entry.Index,
                    entry.Card.Suit.ToOperation(),
                    entry.Card.Rank,
                    before,
                    total
                ));
            }

            // Remove applied cards
            remaining.RemoveAll(e => group.Operators.Contains(e.Card.Suit.ToOperation()));
        }

        return steps;
    }

    public static List<ApplyStep> BuildApplySteps(Hand hand, ResolveProfile profile, double start = 0, bool selected = true)
    {
        var cards = selected ? [.. hand.Selected.Select(c => c.Data)] : hand.Cards.ToList();
        return BuildApplySteps(start, cards, profile);
    }
    private static double Apply(double a, double b, Op op)
    {
        return op switch
        {
            Op.Add => a + b,
            Op.Subtract => a - b,
            Op.Multiply => a * b,
            Op.Divide => b == 0 ? 0 : a / b,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
