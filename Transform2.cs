using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Magabot.Simulator
{
    public sealed class Transform2
    {
        public Vector2 Position;
        public float Rotation;
        public Vector2 Scale;

        public Transform2()
        {
            Scale = Vector2.One;
        }

        public Transform2(Transform2 transform)
            : this(transform.Position, transform.Rotation, transform.Scale)
        {
        }

        public Transform2(Vector2 position, float rotation, Vector2 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}
