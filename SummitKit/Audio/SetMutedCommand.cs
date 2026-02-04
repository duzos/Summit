

using Microsoft.Xna.Framework.Media;
using SummitKit;
using SummitKit.Command;
using System;

namespace SummitKit.Audio;

public sealed class SetMutedCommand : ICommand
{
    public string Name => "set-muted";

    public string Usage => "set-muted <muted>";

    public void Execute(CommandContext context,string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException(Usage);
        }
        if (!context.ParseBool(args[0], out bool muted))
        {
            context.Error("Invalid muted value. Must be true or false.");
            return;
        }
        Core.Audio.SetMuted(muted);
        context.Success($"Audio muted set to {muted}.");
    }
}
