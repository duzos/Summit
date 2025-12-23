using Summit.Maths;
using SummitKit.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Command;

public sealed class SetResolveProfileCommand : ICommand
{
    public string Name => "resolve";

    public string Usage => "resolve <bidmas | order | inverted>";

    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 1)
        {
            context.Error("Usage: " + Usage);
            return;
        }

        string name = args[0].ToLowerInvariant();

        MainGame.State.ResolveProfile = name switch
        {
            "bidmas" => TokenExpression.ResolveProfiles.BIDMAS,
            "order" => TokenExpression.ResolveProfiles.LeftToRight,
            "inverted" => TokenExpression.ResolveProfiles.Inverted,
            _ => null
        } ?? MainGame.State.ResolveProfile;

        context.Success($"Set resolve profile to: {MainGame.State.ResolveProfile.Name}");
    }
}
