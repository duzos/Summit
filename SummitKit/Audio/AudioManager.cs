using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Audio;

public class AudioManager : IDisposable, IUpdating
{
    private readonly List<SoundEffectInstance> _sounds;
    private float _previousSoundVolume;
    private float _previousMusicVolume;

    public bool IsMuted { get; private set; }

    public float MusicVolume
    {
        get {
            if (IsMuted) return 0F;

            return MediaPlayer.Volume;
        }

        set {
            MediaPlayer.Volume = Math.Clamp(value, 0F, 1F);
        }
    }

    public float SoundVolume
    {
        get {
            if (IsMuted) return 0F;
            return SoundEffect.MasterVolume;
        }
        set {
            SoundEffect.MasterVolume = Math.Clamp(value, 0F, 1F);
        }
    }

    public bool IsDiposed { get; private set; }

    public MusicTracker Music { get; }

    public AudioManager()
    {
        _sounds = [];
        Music = new MusicTracker(this);
    }


    ~AudioManager() => Dispose(false);

    public void Update(GameTime deltaTime)
    {
        for (int i = _sounds.Count - 1; i >= 0; i--)
        {
            SoundEffectInstance sound = _sounds[i];

            if (sound.State != SoundState.Stopped) continue;
            if (!sound.IsDisposed)
            {
                sound.Dispose();
            }

            _sounds.RemoveAt(i);
        }

        Music.Update(deltaTime);
    }

    public SoundEffectInstance PlaySound(SoundEffect sound, float volume = 1f, float pitch = 0.0f, float pan = 0.0f, bool isLooped = false)
    {
        SoundEffectInstance instance = sound.CreateInstance();
        instance.Volume = Math.Clamp(volume, 0f, 1f);
        instance.Pitch = Math.Clamp(pitch, -1f, 1f);
        instance.Pan = Math.Clamp(pan, -1f, 1f);
        instance.IsLooped = isLooped;
        instance.Play();
        _sounds.Add(instance);
        return instance;
    } 

    public void PlayMusic(Song song, bool isLooped = false)
    {
        if (MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Stop();
        }

        MediaPlayer.Play(song);
        MediaPlayer.IsRepeating = isLooped;
    }

    public void FadeMusic(Song song, TimeSpan fadeIn, TimeSpan fadeOut)
    {
        Music.FadeInto(song, fadeOut, fadeIn);
    }

    public void Pause()
    {
        MediaPlayer.Pause();

        foreach (SoundEffectInstance sound in _sounds)
        {
            sound.Pause();
        }
    }

    public void Resume()
    {
        MediaPlayer.Resume();
        foreach (SoundEffectInstance sound in _sounds)
        {
            sound.Resume();
        }
    }

    public void Mute()
    {
        if (IsMuted) return;
        _previousMusicVolume = MusicVolume;
        _previousSoundVolume = SoundVolume;
        MusicVolume = 0F;
        SoundVolume = 0F;
        IsMuted = true;
    }

    public void Unmute()
    {
        if (!IsMuted) return;
        MusicVolume = _previousMusicVolume;
        SoundVolume = _previousSoundVolume;
        IsMuted = false;
    }

    public void ToggleMute()
    {
        if (IsMuted)
        {
            Unmute();
        }
        else
        {
            Mute();
        }
    }

    public void SetMuted(bool val)
    {
        if (val)
        {
            Mute();
        }
        else
        {
            Unmute();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (IsDiposed) return;
        if (disposing)
        {
            foreach (SoundEffectInstance sound in _sounds)
            {
                if (!sound.IsDisposed)
                {
                    sound.Dispose();
                }
            }
            _sounds.Clear();
        }
        IsDiposed = true;
    }
}