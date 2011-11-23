using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Magabot.Simulator.Graphics
{
    public class TextSprite
        : Sprite
    {
        public SpriteFont Font { get; set; }

        public string Text { get; set; }
    }
}
