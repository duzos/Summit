using SummitKit;
using SummitKit.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Command;

public sealed class GravityCommand : ICommand
{
    public string Name => "gravity";
    public string Description => "funny gravity & collision. Usage: gravity <true | false>";

    public string Usage => throw new NotImplementedException();

    public void Execute(CommandContext context, string[] args)
    {
        if (!context.ParseBool(args.ElementAtOrDefault(0) ?? "", out bool enable))
        {
            context.Error("Invalid argument. Usage: gravity <true | false>");
            return;
        }

        Core.Entities.LockToScreenBounds = enable;
        foreach (var item in Core.Entities.Entities)
        {
            item.CollidesWithWindowEdges = enable;
            item.HasCollisions = enable;
            item.HasGravity = enable;
        }
    }
}