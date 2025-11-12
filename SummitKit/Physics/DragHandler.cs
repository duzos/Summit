using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

public class DragHandler : IUpdating
{
    private static readonly TimeSpan HoldTime = TimeSpan.FromSeconds(1);
    private TimeSpan _elapsed;
    private Entity? _possible;
    public Entity? Possible
    {
        get => _possible; set
        {
            if (value != _possible)
            {
                _possible = value;
                _elapsed = TimeSpan.Zero;
            }

            _possible = value;
        }
    }

    public Vector2 PossibleOffset { get; set; }

    public Entity? Dragged { get; private set; }
    public Vector2 DragOffset { get; set; }
    public void Update(GameTime time)
    {
        _elapsed += time.ElapsedGameTime;

        if (_possible != null && _elapsed >= HoldTime)
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