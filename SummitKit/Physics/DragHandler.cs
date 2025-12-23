using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

public class DragHandler : IUpdating
{
    private static readonly float Threshold = 10F;
    private Entity? _possible;
    public Entity? Possible
    {
        get => _possible; set
        {
            if (value != _possible)
            {
                _possible = value;
            }

            _possible = value;
        }
    }

    public Vector2 PossibleOffset { get; set; }

    public Entity? Dragged { get; private set; }
    public Vector2 DragOffset { get; set; }
    public void Update(GameTime time)
    {
        if (_possible != null && (Vector2.Distance(Possible.Position + PossibleOffset, Core.Input.Mouse.Position.ToVector2()) > Threshold))
        {
            Dragged = Possible;
            DragOffset = PossibleOffset;

            PossibleOffset = Vector2.Zero;
            Possible = null;
        }
    }

    public void Release()
    {
        Dragged = null;
        Possible = null;
    }
}