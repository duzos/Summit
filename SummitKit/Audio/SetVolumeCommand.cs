

using Microsoft.Xna.Framework.Media;
using SummitKit;
using SummitKit.Command;
using System;

namespace SummitKit.Audio;

public sealed class SetVolumeCommand : ICommand
{
    public string Name => "set-volume";

    public string Usage => "set-volume <volume>";

    public void Execute(CommandContext context,string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException(Usage);
        }
        if (!context.ParseFloat(args[0], out float volume, min: 0f, max: 1f))
        {
            context.Error("Invalid volume. Must be between 0.0 and 1.0.");
            return;
        }
        MediaPlayer.Volume = volume;
        context.Success($"Volume set to {volume * 100}%.");
    }
}
