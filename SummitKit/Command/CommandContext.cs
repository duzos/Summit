using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Command;

public sealed class CommandContext(Action<string, Color> write)
{
    private readonly Action<string, Color> _write = write;

    public void Reply(string message)
        => _write(message, Color.White);

    public bool Success(string message)
    {
        _write(message, Color.LightGreen);
        return true;
    }

    public bool Error(string message)
    {
        _write(message, Color.IndianRed);
        return false;
    }
    public bool ParseInt(string arg, out int val, int? min = null, int? max = null)
    {
        if (!int.TryParse(arg, out val))
        {
            return Error("Invalid value");
        }

        if (min.HasValue && val < min.Value)
        {
            return Error($"Value must be at least {min.Value}");
        }

        if (max.HasValue && val > max.Value)
        {
            return Error($"Value must be at most {max.Value}");
        }

        return true;
    }

    public bool ParseBool(string arg, out bool val)
    {
        if (!bool.TryParse(arg, out val))
        {
            return Error("Invalid value. Must be true or false");
        }
        return true;
    }

    public bool ParseEnum<T>(string arg, out T val) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(arg, true, out val))
        {
            return Error($"Invalid value.Valid values are: { string.Join(", ", Enum.GetNames<T>())}");
        }
        return true;
    }
}
