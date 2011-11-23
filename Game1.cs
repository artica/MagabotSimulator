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
using System.IO.Ports;
using Guibot;
using Guibot.Hardware;
using Guibot.Communication;
using Magabot.Simulator.Graphics;

namespace Magabot.Simulator
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        SpriteRenderer renderer;

        CommunicationComponent transport;
        ProtocolManager protocol;

        Bumper lowerLeftBumper;
        Bumper upperLeftBumper;
        Bumper upperRightBumper;
        Bumper lowerRightBumper;
        CoupledWheels wheels;

        MaxbotixSonar sonar0;
        MaxbotixSonar sonar1;
        MaxbotixSonar sonar2;
        MaxbotixSonar sonar3;
        MaxbotixSonar sonar4;

        InfraredSensor groundSensor0;
        InfraredSensor groundSensor1;
        InfraredSensor groundSensor2;

        LedPwm redLed;
        LedPwm greenLed;
        LedPwm blueLed;

        DifferentialDrive drive;
        DifferentialOdometry odometry;
        ProximityArray proximity;

        TextureSprite bot;
        Transform2 botFrame;

        TextureSprite[] obstacles;
        Transform2[] obstacleFrames;

        Texture2D squareTex;
        Texture2D botTex;
        Texture2D gridTex;
        Texture2D obstacleTex;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            Content.RootDirectory = "Content";
        }

        private void InitalizeSerialCommunication()
        {
            var serial = new SerialPort("COM4");
            serial.Open();

            if (serial.BytesToRead > 0)
            {
                byte[] buffer = new byte[serial.BytesToRead];
                serial.Read(buffer, 0, (int)buffer.Length);
            }

            transport = new SerialCommunication(serial);
            Exiting += delegate { serial.Close(); };
        }

        private void InitializeNetCommunication()
        {
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            InitalizeSerialCommunication();

            lowerLeftBumper = new Bumper();
            upperLeftBumper = new Bumper();
            upperRightBumper = new Bumper();
            lowerRightBumper = new Bumper();
            wheels = new CoupledWheels();
            wheels.ClicksPerTurn = 3900;
            wheels.WheelDistance = 0.345f;
            wheels.WheelRadius = 0.0467f;
            wheels.Updated += new EventHandler(wheels_Updated);

            sonar0 = new MaxbotixSonar();
            sonar1 = new MaxbotixSonar();
            sonar2 = new MaxbotixSonar();
            sonar3 = new MaxbotixSonar();
            sonar4 = new MaxbotixSonar();

            groundSensor0 = new InfraredSensor();
            groundSensor1 = new InfraredSensor();
            groundSensor2 = new InfraredSensor();

            redLed = new LedPwm();
            greenLed = new LedPwm();
            blueLed = new LedPwm();

            drive = new DifferentialDrive(wheels);
            odometry = new DifferentialOdometry(wheels);
            proximity = new ProximityArray
            {
                { new Transform(0.15f, 0.14f, MathHelper.ToRadians(45)), sonar0 },
                { new Transform(0.165f, 0.07f, MathHelper.ToRadians(20)), sonar1 },
                { new Transform(0.18f, 0.0f, MathHelper.ToRadians(0)), sonar2 },
                { new Transform(0.165f, -0.07f, MathHelper.ToRadians(-20)), sonar3 },
                { new Transform(0.15f, -0.14f, MathHelper.ToRadians(-45)), sonar4 },
            };

            protocol = new ProtocolManager();
            protocol.SubscribeComponent(lowerLeftBumper);
            protocol.SubscribeComponent(upperLeftBumper);
            protocol.SubscribeComponent(upperRightBumper);
            protocol.SubscribeComponent(lowerRightBumper);
            protocol.SubscribeComponent(wheels);
            protocol.SubscribeComponent(sonar0);
            protocol.SubscribeComponent(sonar1);
            protocol.SubscribeComponent(sonar2);
            protocol.SubscribeComponent(sonar3);
            protocol.SubscribeComponent(sonar4);
            protocol.SubscribeComponent(groundSensor0);
            protocol.SubscribeComponent(groundSensor1);
            protocol.SubscribeComponent(groundSensor2);
            protocol.SubscribeComponent(greenLed);
            protocol.SubscribeComponent(redLed);
            protocol.SubscribeComponent(blueLed);
            transport.Protocol = protocol;

            renderer = new SpriteRenderer(this);
            renderer.PixelsPerMeter = 100;
            Components.Add(renderer);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            base.Initialize();
        }

        void wheels_Updated(object sender, EventArgs e)
        {
            odometry.Update();

            botFrame.Position.X += (float)(odometry.ForwardDelta * Math.Cos(botFrame.Rotation));
            botFrame.Position.Y += (float)(odometry.ForwardDelta * Math.Sin(botFrame.Rotation));
            botFrame.Rotation += odometry.RotationDelta;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteFont = Content.Load<SpriteFont>("Font");

            squareTex = Content.Load<Texture2D>("square");
            botTex = Content.Load<Texture2D>("magabot_cm");
            gridTex = Content.Load<Texture2D>("grid");
            obstacleTex = DrawingHelper.CreateCircleTexture(GraphicsDevice, 10, Color.Purple, Color.Black);

            bot = new TextureSprite(botTex);
            botFrame = new Transform2();
            renderer.SubscribeTexture(botFrame, bot);

            obstacles = new TextureSprite[proximity.Count];
            obstacleFrames = new Transform2[proximity.Count];
            for (int i = 0; i < obstacles.Length; i++)
            {
                obstacles[i] = new TextureSprite(obstacleTex);
                obstacleFrames[i] = new Transform2();
                renderer.SubscribeTexture(obstacleFrames[i], obstacles[i]);
            }
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        float vel;
        float rot;

        float leftVel;
        float rightVel;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            if (gameTime.TotalGameTime.Milliseconds > 100)
            {
                transport.Update();
            }

            var obstaclePos = proximity.GetObstacles();
            for (int i = 0; i < obstaclePos.Length; i++)
            {
                var pos = new Vector2(obstaclePos[i].X, obstaclePos[i].Y);
                obstacleFrames[i].Position = pos.Rotate(botFrame.Rotation) + botFrame.Position;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                vel = Math.Min(0.05f, vel + 0.001f);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                vel = Math.Max(-0.05f, vel - 0.001f);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                rot = (float)(4 * Math.PI / 360);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                rot = -(float)(4 * Math.PI / 360);
            }
            else rot = 0;

            redLed.SetValue(Mouse.GetState().LeftButton == ButtonState.Pressed ? 0 : 1);
            greenLed.SetValue(Mouse.GetState().MiddleButton == ButtonState.Pressed ? 0 : 1);
            blueLed.SetValue(Mouse.GetState().RightButton == ButtonState.Pressed ? 0 : 1);

            leftVel = MathHelper.SmoothStep(leftVel, wheels.GetLeftVelocity(), 0.05f);
            rightVel = MathHelper.SmoothStep(rightVel, wheels.GetRightVelocity(), 0.05f);

            if (Keyboard.GetState().IsKeyDown(Keys.Back))
            {
                vel = 0;
                rot = 0;
                wheels.SetTargetVelocity(vel, rot);
            }
            else drive.SetTargetVelocity(vel, rot);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var pleftVel = string.Format("Left: {0:F3}", leftVel / 12.0f);
            var prightVel = string.Format("Right: {0:F3}", rightVel / 12.0f);

            var tvel = string.Format("Vel: {0:F3}", vel);
            var rvel = string.Format("Vel: {0:F3}", rot);
            var psonar0 = string.Format("Sonar0: {0:F3}", sonar0.GetDistance());
            var psonar1 = string.Format("Sonar1: {0:F3}", sonar1.GetDistance());
            var psonar2 = string.Format("Sonar2: {0:F3}", sonar2.GetDistance());
            var psonar3 = string.Format("Sonar3: {0:F3}", sonar3.GetDistance());
            var psonar4 = string.Format("Sonar4: {0:F3}", sonar4.GetDistance());
            var pground0 = string.Format("Ground0: {0:F3}", groundSensor0.GetValue());
            var pground1 = string.Format("Ground1: {0:F3}", groundSensor1.GetValue());
            var pground2 = string.Format("Ground2: {0:F3}", groundSensor2.GetValue());

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(gridTex, Vector2.Zero, Color.White);
            //spriteBatch.Draw(squareTex, pos, Color.Red);

            spriteBatch.Draw(squareTex, 50 * Vector2.One, lowerLeftBumper.GetState() ? Color.Black : Color.White);
            spriteBatch.Draw(squareTex, 120 * Vector2.One, upperLeftBumper.GetState() ? Color.Black : Color.White);
            spriteBatch.Draw(squareTex, 190 * Vector2.One, upperRightBumper.GetState() ? Color.Black : Color.White);
            spriteBatch.Draw(squareTex, 260 * Vector2.One, lowerRightBumper.GetState() ? Color.Black : Color.White);

            spriteBatch.DrawString(spriteFont, pleftVel, new Vector2(0, 200), Color.White);
            spriteBatch.DrawString(spriteFont, prightVel, new Vector2(0, 200 + spriteFont.LineSpacing), Color.White);
            spriteBatch.DrawString(spriteFont, tvel, new Vector2(0, 200 + 2 * spriteFont.LineSpacing), Color.White);
            spriteBatch.DrawString(spriteFont, rvel, new Vector2(0, 200 + 3 * spriteFont.LineSpacing), Color.White);

            spriteBatch.DrawString(spriteFont, psonar0, new Vector2(0, 200 + 4 * spriteFont.LineSpacing), Color.White);
            spriteBatch.DrawString(spriteFont, psonar1, new Vector2(0, 200 + 5 * spriteFont.LineSpacing), Color.White);
            spriteBatch.DrawString(spriteFont, psonar2, new Vector2(0, 200 + 6 * spriteFont.LineSpacing), Color.White);
            spriteBatch.DrawString(spriteFont, psonar3, new Vector2(0, 200 + 7 * spriteFont.LineSpacing), Color.White);
            spriteBatch.DrawString(spriteFont, psonar4, new Vector2(0, 200 + 8 * spriteFont.LineSpacing), Color.White);

            spriteBatch.DrawString(spriteFont, pground0, new Vector2(0, 200 + 9 * spriteFont.LineSpacing), Color.White);
            spriteBatch.DrawString(spriteFont, pground1, new Vector2(0, 200 + 10 * spriteFont.LineSpacing), Color.White);
            spriteBatch.DrawString(spriteFont, pground2, new Vector2(0, 200 + 11 * spriteFont.LineSpacing), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
