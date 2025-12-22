

using SummitKit.Command;
using System;

namespace Summit.Command;

public sealed class SetHandCommand : ICommand
{
    public string Name => "set-hand";

    public string Usage => "set-hand <amount>";

    public void Execute(CommandContext context,string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException(Usage);
        }
    
        int amount = int.Parse(args[0]);

        MainGame.State.RemainingHands = amount;
        context.Success($"Remaining hands set to {amount}.");
    }
}
