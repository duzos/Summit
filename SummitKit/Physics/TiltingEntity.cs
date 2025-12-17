using SummitKit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

/// <summary>
/// An extension of <see cref="Entity"/> that supports tilting mechanics.
/// It tilts left and right and scales up and down
/// </summary>
/// <param name="sprite"></param>
public abstract class TiltingEntity(Sprite sprite) : Entity(sprite)
{
}
