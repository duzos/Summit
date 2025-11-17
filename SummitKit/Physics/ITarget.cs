using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

public interface ITarget : IUpdating
{
    bool IsComplete { get; }
    Vector2 From { get; set; }
    Vector2 To { get; }
    Action<ITarget> Callback { get; set; }
}
