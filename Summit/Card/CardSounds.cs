using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using SummitKit;
using SummitKit.Audio;
using SummitKit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Card;

public class CardSounds : ILoading
{
    private readonly List<SoundEffect> _place = [];
    private readonly List<SoundEffect> _flick = [];
    private readonly List<SoundEffect> _trigger = [];
    private readonly AudioManager _audio = Core.Audio;
    private static readonly Random _random = new();

    public void LoadContent(ContentManager content)
    {
        for (int i = 1; i <= 5; i++)
        {
            _place.Add(LoadSound(content, $"card_place_{i}"));
        }

        _flick.Add(LoadSound(content, "card_flick"));
        _trigger.Add(LoadSound(content, "card_trigger_1"));
    }

    private static SoundEffect LoadSound(ContentManager content, string name)
    {
        return content.Load<SoundEffect>($"assets/audio/card/{name}");
    }

    public SoundEffectInstance PlayPlace()
    {
        var sound = _place[_random.Next(0, _place.Count)];
        
        return _audio.PlaySound(sound, volume: 0.25f + (float)_random.NextDouble(), pitch: -0.2f + (float)_random.NextDouble());
    }

    public SoundEffectInstance PlayFlick()
    {
               var sound = _flick[_random.Next(0, _flick.Count)];
        return _audio.PlaySound(sound, volume: 0.5f + (float)_random.NextDouble(), pitch: -0.2f + (float)_random.NextDouble());
    }

    public SoundEffectInstance PlayTrigger(int index = 0)
    {
        // index increases the pitch
        var sound = _trigger[_random.Next(0, _trigger.Count)];
        return _audio.PlaySound(sound, volume: 0.5f + (float)_random.NextDouble(), pitch: 0.075f * index);
    }
}