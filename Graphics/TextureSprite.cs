using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Magabot.Simulator.Graphics
{
    public class TextureSprite
        : Sprite
    {
        public TextureSprite()
        {
        }

        public TextureSprite(Texture2D texture)
        {
            Texture = texture;
            Origin = new Vector2(texture.Width / 2, texture.Height / 2);
        }

        public Texture2D Texture { get; set; }

        public Rectangle? SourceRectangle { get; set; }
    }
}
