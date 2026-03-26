using Summit.Card;
using SummitKit.Command;
using System;
namespace Summit.Command;

public sealed class SetSeedCommand : ICommand
{
    public string Name => "seed";
    public string Usage => "seed [set] <value>";
    public void Execute(CommandContext ctx, string[] args)
    {
        // no args = print seed
        if (args.Length == 0)
        {
            ctx.Reply($"Seed: {MainGame.State.Seed}");
            return;
        }

        // set seed
        if (args.Length == 2 && args[0] == "set")
        {
            if (ctx.ParseInt(args[1], out int seed))
            {
                MainGame.State.Seed = seed;
                ctx.Success($"Seed set to {seed}");
            }
            else
            {
                ctx.Error("Invalid seed value");
            }
            return;
        }
    }
}