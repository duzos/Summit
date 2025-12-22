using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Command;

struct ConsoleLine(string text, Color color)
{
    public string Text = text;
    public Color Color = color;
}