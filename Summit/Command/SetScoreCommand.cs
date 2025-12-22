

using SummitKit.Command;
using System;

namespace Summit.Command;

public sealed class SetScoreCommand : ICommand
{
    public string Name => "set-score";

    public string Usage => "set-score <amount>";

    public void Execute(CommandContext context,string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException(Usage);
        }
    
        int amount = int.Parse(args[0]);

        MainGame.State.Score = amount;
        context.Success($"Score set to {amount}.");
    }
}
