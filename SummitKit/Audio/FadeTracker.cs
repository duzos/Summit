using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Audio;

public class FadeTracker : IUpdating
{
    private TimeSpan _elapsed = TimeSpan.Zero;
    public TimeSpan FadeDuration { get; set; } = TimeSpan.FromSeconds(1);
    public float StartVolume { get; set; } = 1.0f;
    public float TargetVolume { get; set; } = 0.0f;
    public float CurrentVolume => (float)(_elapsed.TotalMilliseconds / FadeDuration.TotalMilliseconds) * (TargetVolume - StartVolume) + StartVolume;
    public bool IsComplete => NextSong is null && _elapsed >= FadeDuration;

    public Song? NextSong { get; set; } = null;
    public Action<Song> PlaySong { get; set; } = (song) => Core.Audio.PlayMusic(song);
    public Action? OnComplete { get; set; } = null;

    public void Update(GameTime deltaTime)
    {
        _elapsed += deltaTime.ElapsedGameTime;

        if (_elapsed >= FadeDuration)
        {
            MediaPlayer.Volume = TargetVolume;
            // now fade into next song if applicable
            if (NextSong is not null && PlaySong is not null)
            {
                PlaySong.Invoke(NextSong);
                NextSong = null;
                _elapsed = TimeSpan.Zero;
                (TargetVolume, StartVolume) = (StartVolume, TargetVolume);
            }

            OnComplete?.Invoke();
        }
        else
        {
            MediaPlayer.Volume = CurrentVolume;
        }
    }
}