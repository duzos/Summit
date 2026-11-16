using Microsoft.Xna.Framework;
using SummitKit.Command;
using SummitKit.Physics;
using SummitKit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Graphics;
public sealed class CameraCommand : ICommand
{
    public string Name => "camera";

    public string Usage => "camera <zoom> <val>";

    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 1)
        {
            context.Error("Usage: " + Usage);
            return;
        }

        if (args[0] == "zoom")
        {
            if (args.Length < 2)
            {
                context.Error("Usage: " + Usage);
                return;
            }
            if (!context.ParseFloat(args[1], out float zoom, 1))
            {
                context.Error("Invalid zoom value.");
                return;
            }

            Core.Camera.ZoomTo(new InterpolatedTarget<float>(zoom, Core.Camera.Zoom, v => Core.Camera.Zoom = v, TimeSpan.FromSeconds(1), TimeSpan.Zero, MathHelper.Lerp));

            context.Success("Camera zoom set to " + zoom);
            return;
        }
    }
}