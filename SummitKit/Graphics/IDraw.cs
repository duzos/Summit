using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Graphics;
public interface IDraw
{
    float LayerDepth { get; set; }
    void Draw(SpriteBatch spriteBatch);
}
