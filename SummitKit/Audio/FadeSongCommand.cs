

using Microsoft.Xna.Framework.Media;
using SummitKit;
using SummitKit.Command;
using System;

namespace SummitKit.Audio;

public sealed class FadeSongCommand : ICommand
{
    public string Name => "set-song";

    public string Usage => "set-song [name] [fade-in] [fade-out]";

    public void Execute(CommandContext context,string[] args)
    {
        MusicTracker music = Core.Audio.Music;

        // if no args, list available songs
        if (args.Length == 0)
        {
            context.Reply("Available songs: ");
            foreach (string name in music.SongNames)
            {
                context.Reply($"- {name}");
            }
            return;
        }

        // parse args
        string songName = args[0];
        TimeSpan fadeIn = TimeSpan.FromSeconds(1);
        TimeSpan fadeOut = TimeSpan.FromSeconds(1);

        if (args.Length >= 2)
        {
            if (!context.ParseInt(args[1], out int fadeInSeconds, min: 0))
            {
                context.Error("Invalid fade-in time.");
                return;
            }
            fadeIn = TimeSpan.FromSeconds(fadeInSeconds);
        }

        if (args.Length >= 3)
        {
            if (!context.ParseInt(args[2], out int fadeOutSeconds, min: 0))
            {
                context.Error("Invalid fade-out time.");
                return;
            }
            fadeOut = TimeSpan.FromSeconds(fadeOutSeconds);
        }

        // find song
        Song? song = music.FromName(songName);
        if (song is null)
        {
            context.Error($"Song '{songName}' not found.");
            return;
        }

        // fade into song
        music.FadeInto(song, fadeOut, fadeIn);
    }
}
