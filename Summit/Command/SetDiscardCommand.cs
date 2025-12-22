

using SummitKit.Command;
using System;

namespace Summit.Command;

public sealed class SetDiscardCommand : ICommand
{
    public string Name => "set-discard";

    public string Usage => "set-discard <amount>";

    public void Execute(CommandContext context,string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException(Usage);
        }
    
        int amount = int.Parse(args[0]);

        MainGame.State.RemainingDiscards = amount;
        context.Success($"Remaining discards set to {amount}.");
    }
}
