using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Magabot.Simulator
{
    public static class Extensions
    {
        public static Vector2 Rotate(this Vector2 vector, float rotation)
        {
            var sinRot = (float)Math.Sin(rotation);
            var cosRot = (float)Math.Cos(rotation);

            return new Vector2(
                vector.X * cosRot - vector.Y * sinRot,
                vector.X * sinRot + vector.Y * cosRot
            );
        }
    }
}
