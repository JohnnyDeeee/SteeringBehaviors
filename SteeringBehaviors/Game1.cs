using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;

namespace SteeringBehaviors {
    public class Obstacle {
        public Vector2 position { get; private set; }
        public float radius { get; private set; }

        // TEMP
        public Color color { get; set; }

        public Obstacle(Vector2 position, float radius) {
            this.position = position;
            this.radius = radius;
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            spriteBatch.DrawCircle(position, radius, 180, color);
            spriteBatch.End();
        }
    }

    public class Creature {
        // Movement properties
        private Vector2 position;
        private Vector2 velocity = new Vector2(0, 0);
        private Vector2 acceleration = new Vector2(0, 0);
        private float maxForce = 0.2f;
        private float maxSpeed = 2f;//5f;
        // Visual properties
        private int width;
        private int height;
        private Polygon shape;
        private float rotation;
        // Avoidance properties
        private Vector2 ahead;
        private float visionLength = 200f;

        public Creature(Vector2 position, int size, Color color) {
            this.position = position;
            width = size;
            height = size + 5;
        }

        public void Update(Vector2 target, List<Obstacle> obstacles) {
            // Keep tracking the target
            Vector2 seekForce = Seek(target);

            // Avoid any obstacles
            Vector2 avoidForce = Avoid(obstacles);

            // Movement
            acceleration += avoidForce != Vector2.Zero ? avoidForce : seekForce; // avoidForce has prio over seekForce
            velocity += acceleration;
            velocity = velocity.Truncate(maxSpeed);
            position += velocity;
            acceleration *= 0;

            // Important to draw shape on every update, because the rotation of the shape is dependant on where the target is
            shape = CreateShape();

            // Create the vision vector
            ahead = position + Vector2.Normalize(velocity) * visionLength;
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            spriteBatch.DrawPolygon(position, shape, Color.White);
            spriteBatch.DrawLine(position, ahead, Color.Red, 2f);
            spriteBatch.End();
        }

        private Vector2 Seek(Vector2 targetPosition) {
            Vector2 direction = Vector2.Normalize(targetPosition - position);
            direction *= maxSpeed;
            Vector2 steering = Vector2.Normalize(direction - velocity);
            steering = steering.Truncate(maxForce);

            return steering;
        }

        private Vector2 Avoid(List<Obstacle> obstacles) {
            float closestObstacleDistance = 1 / 0f; // Initially, set it to infinity
            Obstacle closestObstacle = null; // Most threatening
            foreach (Obstacle obstacle in obstacles) {
                // Check if ahead line collides with the obstacle
                NetRumble.CollisionMath.CircleLineCollisionResult result = new NetRumble.CollisionMath.CircleLineCollisionResult();
                NetRumble.CollisionMath.CircleLineCollide(obstacle.position, obstacle.radius, position, ahead, ref result);
                if (result.Collision) { // Collision
                    float distanceCreatureToObstacle = Vector2.Distance(position, obstacle.position);

                    // Check if this is the closest obstacle with respect to the creature's position
                    if (distanceCreatureToObstacle < closestObstacleDistance) { // Most threatening (closest)
                        closestObstacleDistance = distanceCreatureToObstacle;
                        closestObstacle = obstacle;
                        obstacle.color = Color.Red;
                    } else // Less threatening (further away)
                        obstacle.color = Color.Orange;
                } else // No collision
                    obstacle.color = Color.Green;
            }

            if (closestObstacle == null)
                return Vector2.Zero;

            Vector2 avoidanceForce = Vector2.Normalize(ahead - closestObstacle.position);
            avoidanceForce = avoidanceForce.Truncate(maxForce);
            return avoidanceForce;
        }

        private Polygon CreateShape() {
            rotation = (float)(Math.Atan2(velocity.Y, velocity.X) + Math.PI / 2); // Rotation towards the velocity

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
        private List<Obstacle> obstacles = new List<Obstacle>();
        private Vector2 target;
        private MouseState previousMouseState;

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
            spriteBatch.Dispose();
        }

        protected override void Update(GameTime gameTime) {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                target = Mouse.GetState().Position.ToVector2();
            if (Mouse.GetState().RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed)
                obstacles.Add(new Obstacle(Mouse.GetState().Position.ToVector2(), 55));

            previousMouseState = Mouse.GetState();

            foreach (Creature creature in creatures) {
                creature.Update(target, obstacles);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(new Color(45, 52, 54, 1));

            foreach (Creature creature in creatures) {
                creature.Draw(spriteBatch);
            }

            foreach (Obstacle obstacle in obstacles) {
                obstacle.Draw(spriteBatch);
            }

            spriteBatch.Begin();
            spriteBatch.DrawPoint(target, Color.Red, 10f); // DEBUG
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
