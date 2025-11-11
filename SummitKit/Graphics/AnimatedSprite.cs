using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Graphics;

public class AnimatedSprite : Sprite
{
    private int _currentFrame;
    private TimeSpan _elapsed;
    private Animation _animation;
    private Type _playType = Type.LOOP;
    public Type PlayType
    {
        get => _playType;
        set => _playType = value;
    }
    private int delta = 1;

    /// <summary>
    /// Creates a new animated sprite.
    /// </summary>
    public AnimatedSprite() { 
        
    }

    /// <summary>
    /// Creates a new animated sprite with the specified frames and delay.
    /// </summary>
    /// <param name="animation">The animation for this animated sprite.</param>
    public AnimatedSprite(Animation animation) : this()
    {
        Animation = animation;
    }

    /// <summary>
    /// Gets or Sets the animation for this animated sprite.
    /// </summary>
    public Animation Animation
    {
        get => _animation;
        set
        {
            _animation = value;
            Region = _animation.Frames[0];
        }
    }

    public void Reverse()
    {
        delta *= -1;
    }

    public void Pause()
    {
        delta = 0;
    }

    public void Continue()
    {
        if (delta == 0)
        {
            delta = 1;
        }
    }

    /// <summary>
    /// Updates this animated sprite.
    /// </summary>
    /// <param name="gameTime">A snapshot of the game timing values provided by the framework.</param>
    public override void Update(GameTime gameTime)
    {
        _elapsed += gameTime.ElapsedGameTime;

        if (_elapsed >= _animation.Delay)
        {
            _elapsed -= _animation.Delay;
            _currentFrame += delta;

            if (_currentFrame >= _animation.Frames.Count)
            {
                switch (PlayType)
                {
                    case Type.HOLD:
                        delta = 0;
                        _currentFrame--;
                        break;
                    case Type.LOOP:
                        _currentFrame = 0;
                        break;
                    case Type.BOUNCE:
                        _currentFrame-=2;
                        Reverse();
                        break;
                }
            }

            if (_currentFrame <= 0)
            {
                switch (PlayType)
                {
                    case Type.HOLD:
                        delta = 0;
                        _currentFrame = 0;
                        break;
                    case Type.LOOP:
                        if (_currentFrame < 0)
                        {
                            _currentFrame = _animation.Frames.Count - 1;
                        }
                        break;
                    case Type.BOUNCE:
                        Reverse();
                        _currentFrame = 0;
                        break;
                }
            }

            Region = _animation.Frames[_currentFrame];
        }
    }

    public enum Type
    {
        HOLD,
        LOOP,
        BOUNCE
    }
}
