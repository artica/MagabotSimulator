using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Guibot;


namespace Magabot.Simulator.Graphics
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SpriteRenderer : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private Matrix projection;
        private SpriteBatch spriteBatch;
        private event Action draw;
        private static readonly Vector2 scaleCorrection = new Vector2(1, -1);

        public SpriteRenderer(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            SortMode = SpriteSortMode.Deferred;
            PixelsPerMeter = 100;
        }

        public int PixelsPerMeter { get; set; }

        public SpriteSortMode SortMode { get; set; }

        public BlendState BlendState { get; set; }

        public SamplerState SamplerState { get; set; }

        public DepthStencilState DepthStencilState { get; set; }

        public RasterizerState RasterizerState { get; set; }

        public Effect Effect { get; set; }

        public IDisposable SubscribeTexture(Transform2 transform, TextureSprite sprite)
        {
            Action handler = () => spriteBatch.Draw(
                sprite.Texture,
                PixelsPerMeter * transform.Position,
                sprite.SourceRectangle,
                sprite.Color,
                transform.Rotation,
                sprite.Origin,
                scaleCorrection * transform.Scale,
                sprite.Effects,
                sprite.LayerDepth);

            draw += handler;
            return Disposable.Create(() => draw -= handler);
        }

        public IDisposable SubscribeText(Transform2 transform, TextSprite sprite)
        {
            Action handler = () => spriteBatch.DrawString(
                sprite.Font,
                sprite.Text,
                PixelsPerMeter * transform.Position,
                sprite.Color,
                transform.Rotation,
                sprite.Origin,
                scaleCorrection * transform.Scale,
                sprite.Effects,
                sprite.LayerDepth);

            draw += handler;
            return Disposable.Create(() => draw -= handler);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            base.Initialize();
        }

        protected override void LoadContent()
        {
            var viewport = GraphicsDevice.Viewport;
            projection =
                Matrix.CreateScale(1, -1, 1) *
                Matrix.CreateTranslation(viewport.Width * .5f, viewport.Height * .5f, 0);

            spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            base.Update(gameTime);
        }

        private void OnDraw()
        {
            var handler = draw;
            if (handler != null)
            {
                handler();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SortMode, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect, projection);
            OnDraw();
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Conversion Methods

        public int GetScreenCoord(float worldCoord)
        {
            return (int)(worldCoord * PixelsPerMeter);
        }

        public Point GetScreenPoint(float x, float y)
        {
            return GetScreenPoint(new Vector2(x, y));
        }

        public Point GetScreenPoint(Vector2 worldPoint)
        {
            return new Point(GetScreenCoord(worldPoint.X), GetScreenCoord(worldPoint.Y));
        }

        #endregion
    }
}
