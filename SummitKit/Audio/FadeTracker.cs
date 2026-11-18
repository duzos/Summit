using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using SummitKit.Physics;
using System;

namespace SummitKit.Audio;

public class FadeTracker : IUpdating
{
    private TimeSpan _elapsed = TimeSpan.Zero;
    public TimeSpan FadeDuration { get; set; } = TimeSpan.FromSeconds(1);
    public float StartLevel { get; set; } = 1.0f;
    public float TargetLevel { get; set; } = 0.0f;
    public float ReturnLevel { get; set; } = 1.0f;
    public float CurrentLevel => FadeDuration <= TimeSpan.Zero
        ? TargetLevel
        : (float)(_elapsed.TotalMilliseconds / FadeDuration.TotalMilliseconds) * (TargetLevel - StartLevel) + StartLevel;
    public bool IsComplete => NextSong is null && _elapsed >= FadeDuration;

    public Song? NextSong { get; set; } = null;
    public Action<Song> PlaySong { get; set; } = (song) => Core.Audio.PlayMusic(song);
    public Action? OnComplete { get; set; } = null;

    public void Update(GameTime deltaTime)
    {
        _elapsed += deltaTime.ElapsedGameTime;

        if (_elapsed >= FadeDuration)
        {
            Core.Audio.MusicLevel = TargetLevel;
            if (NextSong is not null && PlaySong is not null)
            {
                PlaySong.Invoke(NextSong);
                NextSong = null;
                _elapsed = TimeSpan.Zero;
                StartLevel = TargetLevel;
                TargetLevel = ReturnLevel;
            }

            OnComplete?.Invoke();
        }
        else
        {
            Core.Audio.MusicLevel = CurrentLevel;
        }
    }
}
