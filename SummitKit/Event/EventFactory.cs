using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Event;

public static class EventFactory
{
    public static Event<T> CreateEvent<T>(Func<T[], T> factory)
    {
        return new ArrayBackedEvent<T>(factory);
    }
}