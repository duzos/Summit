using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

public interface IPositioned
{
    ITarget<Vector2>? MoveTarget { get; }
    Vector2 Position { get; set; }
    float Width { get; }
    float Height { get; }

    void MoveTo(ITarget<Vector2> target, bool replaceExisting = true);
}