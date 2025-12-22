

using SummitKit.Command;
using System;

namespace Summit.Command;

public sealed class SetTargetScoreCommand : ICommand
{
    public string Name => "set-target-score";

    public string Usage => "set-target-score <amount>";

    public void Execute(CommandContext context,string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException(Usage);
        }
    
        int amount = int.Parse(args[0]);

        MainGame.State.TargetScore = amount;
        context.Success($"Target score set to {amount}.");
    }
}
