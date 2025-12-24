using SummitKit.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.UI;

public sealed class DebugUICommand : ICommand
{
    public string Name => "debug-ui";

    public string Usage => "debug-ui <enabled | thickness> [value]";

    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 1)
        {
            context.Error("Usage: " + Usage);
            return;
        }

        if (args[0] == "thickness")
        {
            if (args.Length < 2)
            {
                context.Error("Usage: " + Usage);
                return;
            }
            if (!context.ParseInt(args[1], out int thickness, 1))
            {
                context.Error("Invalid thickness value.");
                return;
            }
            IUIElement.Thickness = thickness;
            context.Success("UI Debug thickness set to " + thickness);
            return;
        }

        if (!context.ParseBool(args[0], out bool enabled))
        { 
            return;
        }

        IUIElement.Debug = enabled;
        context.Success("UI Debug mode " + (enabled ? "enabled" : "disabled"));
    }
}