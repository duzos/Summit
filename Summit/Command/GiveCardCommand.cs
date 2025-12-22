using Summit.Card;
using SummitKit.Command;
using System;
namespace Summit.Command;

public sealed class GiveCardCommand : ICommand
{
    public string Name => "givecard";
    public string? Description => "Puts the given card on top of your deck or into your hand";
    public string Usage => "givecard <suit> <value> [inHand]";
    public void Execute(CommandContext ctx, string[] args)
    {
        if (args.Length < 2)
        {
            ctx.Error("Usage: " + Usage);
            return;
        }

        if (!ctx.ParseEnum(args[0], out CardSuit suit)) return;
        if (!ctx.ParseInt(args[1], out var value, 1, 13)) return;
        if (!ctx.ParseBool(args.Length >= 3 ? args[2] : "false", out var inHand)) return;

        var data = new CardData(value, suit);
        var state = MainGame.State;

        if (inHand)
        {
            state.MainHand.AddCard(data, true);
            state.MainHand.SpawnCards();
            ctx.Success($"Added {data.GetName()} to your hand.");
        } else
        {
            state.MainDeck.AddAllToTop([data]);
            ctx.Success($"Added {data.GetName()} to the top of your deck.");
        }
    }
}