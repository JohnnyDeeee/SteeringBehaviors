using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;

namespace SteeringBehaviors {
    public class Game1 : Game {
        public static GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Random random = new Random();
        private List<Creature> creatures = new List<Creature>();
        private List<Obstacle> obstacles = new List<Obstacle>();
        private Target target;
        private MouseState previousMouseState;
        private KeyboardState previousKeyboardState;
        private SpriteFont font;

        public static bool Debug = true;

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            this.IsMouseVisible = true;

            target = new Target(new Vector2(random.Next(graphics.PreferredBackBufferWidth), random.Next(graphics.PreferredBackBufferHeight)));
            creatures.Add(new Creature(
                new Vector2(0, 0),
                15,
                Color.Green
            ));

            base.Initialize();
        }

        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");
        }

        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
            spriteBatch.Dispose();
        }

        protected override void Update(GameTime gameTime) {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyUp(Keys.OemTilde) && previousKeyboardState.IsKeyDown(Keys.OemTilde))
                Game1.Debug = !Game1.Debug;
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                target.colliderPosition = Mouse.GetState().Position.ToVector2();
            if (Mouse.GetState().RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed)
                obstacles.Add(new Obstacle(Mouse.GetState().Position.ToVector2(), 55));

            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();

            foreach (Creature creature in creatures) {
                creature.Update(target, obstacles);
            }

            target.Update(creatures);

            if (random.NextDouble() < 0.004f)
                obstacles.Add(new Obstacle(new Vector2(random.Next(graphics.PreferredBackBufferWidth), random.Next(graphics.PreferredBackBufferHeight)), random.Next(25, 50)));

            if (random.NextDouble() < 0.004f && obstacles.Count > 0)
                obstacles.RemoveAt(0);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(new Color(45, 52, 54, 1));

            // Objects
            foreach (Creature creature in creatures) {
                creature.Draw(spriteBatch);
            }

            foreach (Obstacle obstacle in obstacles) {
                obstacle.Draw(spriteBatch);
            }

            target.Draw(spriteBatch);

            // UI
            spriteBatch.Begin();
            spriteBatch.DrawString(font, $"Press ~ to turn debug mode {(Game1.Debug ? "off" : "on")}", new Vector2(10, 400), Color.White);
            spriteBatch.DrawString(font, $"Press Left mouse button to move the target", new Vector2(10, 420), Color.White);
            spriteBatch.DrawString(font, $"Press Right mouse button to place an obstacle", new Vector2(10, 440), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
