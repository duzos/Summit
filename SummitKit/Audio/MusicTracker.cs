using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Audio;

public class MusicTracker(AudioManager audio) : IUpdating
{
    private readonly AudioManager _audio = audio;
    private readonly List<Song> _songs = [];
    private static readonly Random random = new();

    public static Song CurrentMusic
    {
        get
        {
            return MediaPlayer.Queue.ActiveSong;
        }

        set
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            MediaPlayer.Play(value);
        }
    }

    public bool IsRandomised { get; set; } = true;
    public bool RemoveOnPlay { get; set; } = false;

    public void Update(GameTime deltaTime)
    {
        if (MediaPlayer.State == MediaState.Stopped && _songs.Count > 0)
        {
            Song nextSong;
            if (IsRandomised)
            {
                int index = random.Next(0, _songs.Count);
                nextSong = _songs[index];
            }
            else
            {
                nextSong = _songs[0];
            }
            if (RemoveOnPlay)
            {
                _songs.Remove(nextSong);
            }
            CurrentMusic = nextSong;
        }
    }

    public void AddSong(Song song)
    {
        _songs.Add(song);
    }
}