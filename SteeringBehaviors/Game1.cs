using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteeringBehaviors {
    public class Game1 : Game {
        public static GraphicsDeviceManager graphics;
        public static bool Debug = false;

        private bool randomObstacleMode = false;

        private SpriteBatch spriteBatch;
        private Random random = new Random();
        private List<Creature> creatures = new List<Creature>();
        private List<Obstacle> obstacles = new List<Obstacle>();
        private Target target;
        private MouseState previousMouseState;
        private KeyboardState previousKeyboardState;
        private SpriteFont font;
        private int creatureAmount = 1;
        private int maxFrameRate = 30;

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public Vector2 RandomVector() {
            return new Vector2(random.Next(graphics.PreferredBackBufferWidth), random.Next(graphics.PreferredBackBufferHeight));
        }

        protected override void Initialize() {
            this.IsMouseVisible = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / maxFrameRate);

            base.Initialize();
        }

        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create target and creature(s)
            target = new Target(RandomVector(), spriteBatch);
            for (int i = 0; i < creatureAmount; i++) {
                creatures.Add(new Creature(
                    RandomVector(),
                    10,
                    Color.LightBlue,
                    spriteBatch
                ));
            }

            // Load custom font
            font = Content.Load<SpriteFont>("font");
        }

        protected override void UnloadContent() {
            spriteBatch.Dispose();
        }

        protected override void Update(GameTime gameTime) {
            // Handle inputs
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) // ESCAPE - Exit
{
                Exit();
            }

            if (Keyboard.GetState().IsKeyUp(Keys.OemTilde) && previousKeyboardState.IsKeyDown(Keys.OemTilde)) // TILDE - Debug
{
                Game1.Debug = !Game1.Debug;
            }

            if (Keyboard.GetState().IsKeyUp(Keys.D) && previousKeyboardState.IsKeyDown(Keys.D)) // D - Random obstacle mode
{
                randomObstacleMode = !randomObstacleMode;
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed) // LEFT_BUTTON - Target move
{
                target.colliderPosition = Mouse.GetState().Position.ToVector2();
            }

            if (Keyboard.GetState().IsKeyUp(Keys.OemPlus) && previousKeyboardState.IsKeyDown(Keys.OemPlus)) // PLUS(+) - Add debug creature
{
                creatures.Add(new Creature(Mouse.GetState().Position.ToVector2(), 10, Color.Blue, spriteBatch, true));
            }

            if (Mouse.GetState().RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed) // RIGHT_BUTTON - Add obstacle
{
                obstacles.Add(new Obstacle(Mouse.GetState().Position.ToVector2(), 55, spriteBatch));
            }

            // Save inputs for access in next frame
            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();

            // Update objects
            foreach (Creature creature in creatures) {
                creature.Update(target, creatures.Concat<Collider>(obstacles).ToList<Collider>());
            }

            target.Update(creatures);

            if (randomObstacleMode) {
                // Add random obstacles
                if (random.NextDouble() < 0.02f) {
                    obstacles.Add(new Obstacle(new Vector2(random.Next(graphics.PreferredBackBufferWidth), random.Next(graphics.PreferredBackBufferHeight)), random.Next(25, 50), spriteBatch));
                }

                // Delete random obstacles
                if (random.NextDouble() < 0.005f && obstacles.Count > 0) {
                    obstacles.RemoveAt(0);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(new Color(45, 52, 54, 1));

            // Draw Objects
            foreach (Creature creature in creatures) {
                creature.Draw();
            }

            foreach (Obstacle obstacle in obstacles) {
                obstacle.Draw();
            }

            target.Draw();

            // Draw UI
            spriteBatch.Begin();
            spriteBatch.DrawString(font, $"Press ~ to turn debug mode {(Game1.Debug ? "off" : "on")}", new Vector2(10, 400), Color.White);
            spriteBatch.DrawString(font, $"Press D to turn random obstacle mode {(randomObstacleMode ? "off" : "on")}", new Vector2(10, 420), Color.White);
            spriteBatch.DrawString(font, $"Press Left mouse button to move the target", new Vector2(10, 440), Color.White);
            spriteBatch.DrawString(font, $"Press Right mouse button to place an obstacle", new Vector2(10, 460), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
