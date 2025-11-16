using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Event;

class ArrayBackedEvent<T> : Event<T>
{
    private readonly Func<T[], T> factory;
    private T[] listeners = [];

    /// <summary>
    /// internal use only
    /// </summary>
    public ArrayBackedEvent(Func<T[], T> func)
    {
        factory = func;
        Update();
    }

    public override void Register(T listener)
    {
        var newListeners = new T[listeners.Length + 1];
        Array.Copy(listeners, newListeners, listeners.Length);
        newListeners[^1] = listener;
        listeners = newListeners;
        Update();
    }

    void Update()
    {
        Invoker = factory(listeners);
    }
}