using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Event;

public abstract class Event<T>
{
    public T Invoker { get; protected set; }

    public abstract void Register(T listener);
}