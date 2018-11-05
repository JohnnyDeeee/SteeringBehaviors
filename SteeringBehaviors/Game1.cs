using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;

namespace SteeringBehaviors {
    public class Creature {
        // Movement properties
        public Vector2 position;
        public Vector2 velocity = new Vector2(0, 0);
        public Vector2 acceleration = new Vector2(0, 0);
        public float maxForce = 0.2f;
        public float maxSpeed = 5f;
        // Visual properties
        public int width;
        public int height;
        private Polygon shape;

        public Creature(Vector2 position, int size, Color color) {
            this.position = position;
            width = size;
            height = size + 5;
        }

        public void Update(Vector2 target) {
            // Movement
            velocity += acceleration;
            velocity = velocity.Truncate(maxSpeed);
            position += velocity;
            acceleration *= 0;

            // Keep tracking the target
            Seek(target);

            // Important to draw shape on every update, because the rotation of the shape is dependant on where the target is
            shape = CreateShape();
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            spriteBatch.DrawPolygon(position, shape, Color.White);
            spriteBatch.DrawPoint(position, Color.Red, 2f); // DEBUG
            spriteBatch.End();
        }

        private void ApplyForce(Vector2 forceDirection) {
            acceleration += forceDirection;
        }

        private void Seek(Vector2 targetPosition) {
            Vector2 direction = Vector2.Normalize(targetPosition - position);
            direction *= maxSpeed;
            Vector2 steering = Vector2.Normalize(direction - velocity);
            steering = steering.Truncate(maxForce);

            ApplyForce(steering);
        }

        private Polygon CreateShape() {
            float rotation = (float)(Math.Atan2(velocity.Y, velocity.X) + Math.PI / 2); // Rotation towards the velocity

            Vector2 pointLeft = new Vector2(-width / 2, height / 2); // Left point of the triangle
            pointLeft = Vector2.Transform(pointLeft, Matrix.CreateRotationZ(rotation)); // Calculate new vector when rotating
            pointLeft += new Vector2(0, 0); // Add the origin point

            Vector2 pointRight = new Vector2(width / 2, height / 2); // Right point of the triangle
            pointRight = Vector2.Transform(pointRight, Matrix.CreateRotationZ(rotation)); // Calculate new vector when rotating
            pointRight += new Vector2(0, 0); // Add the origin point

            Vector2 pointTop = new Vector2(0, -height / 2); // Top point of the triangle
            pointTop = Vector2.Transform(pointTop, Matrix.CreateRotationZ(rotation)); // Calculate new vector when rotating
            pointTop += new Vector2(0, 0); // Add the origin point

            // Connect all points with lines to create a Polygon
            Polygon shape = new Polygon(new Vector2[3] {
                pointLeft, // Left
                pointRight, // Right
                pointTop // Top
            });

            return shape;
        }
    }

    public class Game1 : Game {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Random random = new Random();
        private List<Creature> creatures = new List<Creature>();
        private Vector2 target;

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            this.IsMouseVisible = true;

            target = new Vector2(random.Next(graphics.PreferredBackBufferWidth), random.Next(graphics.PreferredBackBufferHeight));
            creatures.Add(new Creature(
                new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2),
                15,
                Color.Green
            ));

            base.Initialize();
        }

        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime) {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                target = Mouse.GetState().Position.ToVector2();

            foreach (Creature creature in creatures) {
                creature.Update(target);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(new Color(45, 52, 54, 1));

            foreach (Creature creature in creatures) {
                creature.Draw(spriteBatch);
            }

            spriteBatch.Begin();
            spriteBatch.DrawPoint(target, Color.Red, 10f); // DEBUG
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
