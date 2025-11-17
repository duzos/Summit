using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

public interface ITarget<T> : IUpdating
{
    bool IsComplete { get; }
    T From { get; set; }
    T To { get; }
    Action<ITarget<T>> Callback { get; set; }
}
