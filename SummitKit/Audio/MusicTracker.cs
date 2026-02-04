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

    public FadeTracker? CurrentFade { get; set; } = new();

    public IEnumerable<Song> Songs => _songs.AsReadOnly();
    public IEnumerable<string> SongNames => _songs.Select(s => s.Name);

    public Song? FromName(string name)
    {
        return _songs.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

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

    public Song? RandomSong => _songs.Count == 0 ? null : _songs[random.Next(0, _songs.Count)];

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

        CurrentFade?.Update(deltaTime);

        if (CurrentFade is not null && CurrentFade.IsComplete)
        {
            CurrentFade = null; 
        }
    }

    public void AddSong(Song song)
    {
        _songs.Add(song);
    }

    public void FadeInto(Song song, TimeSpan fadeOut, TimeSpan fadeIn)
    {
        // Fade out current song, then when completed, fade into this song
        CurrentFade = new()
        {
            FadeDuration = fadeOut,
            StartVolume = MediaPlayer.Volume,
            TargetVolume = 0.0f,
            NextSong = song,
            OnComplete = () =>
            {
                CurrentFade.FadeDuration = fadeIn;
            }
        };
    }
}