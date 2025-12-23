

using SummitKit.Command;
using System;

namespace Summit.Command;

public sealed class NextRoundCommand : ICommand
{
    public string Name => "next-round";

    public string Usage => "next-round";

    public void Execute(CommandContext context,string[] args)
    {
        MainGame.State.NextRound();
        context.Success("Advanced to the next round.");
    }
}
