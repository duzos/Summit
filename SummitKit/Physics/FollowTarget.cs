using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;
public class FollowTarget(Func<Vector2> to, Func<Vector2> getPosition, Action<Vector2> setPosition, float sharpness = 10f) : ITarget<Vector2>
{
    public Vector2 From { get => getPosition(); set { } }
    public Vector2 To => to();
    public Action<ITarget<Vector2>> Callback { get; set; }
    public bool IsComplete => false;

    public void Update(GameTime time)
    {
        float dt = (float)time.ElapsedGameTime.TotalSeconds;
        setPosition(Vector2.Lerp(From, To, 1f - MathF.Exp(-sharpness * dt)));
    }
}