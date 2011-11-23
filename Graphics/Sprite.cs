using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magabot.Simulator.Graphics
{
    public class Sprite
    {
        public Sprite()
        {
            Color = Color.White;
        }

        public Color Color { get; set; }

        public Vector2 Origin { get; set; }

        public SpriteEffects Effects { get; set; }

        public float LayerDepth { get; set; }
    }
}
