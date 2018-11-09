using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using System;
using System.Collections.Generic;
using static NetRumble.CollisionMath;

namespace SteeringBehaviors {
    public class Creature : Collider {
        // Movement properties
        private Vector2 velocity = Vector2.Zero;
        private Vector2 acceleration = Vector2.Zero;
        private float maxForce = 0.2f;
        private float maxSpeed = 2f;//5f;

        // Visual properties
        private int width;
        private int height;
        private Polygon shape;
        private float rotation;
        private Color color;

        // Avoidance properties
        private Vector2 ahead;
        private float visionLength = 90f;

        public Creature(Vector2 position, int size, Color color) : base(position, 10f) {
            this.colliderPosition = position;
            width = size;
            height = size + 5;
            this.color = color;
        }

        public void Update(Target target, List<Obstacle> obstacles) {
            // Keep tracking the target
            Vector2 seekForce = Seek(target.colliderPosition);

            // Avoid any obstacles
            Vector2 avoidForce = Avoid(obstacles);

            // Physics movement
            acceleration += avoidForce != Vector2.Zero ? avoidForce : seekForce; // avoidForce has prio over seekForce
            velocity += acceleration;
            velocity = velocity.Truncate(maxSpeed);
            colliderPosition += velocity;
            acceleration *= 0;

            // TODO: Actually fix the bug where position can be NaN
            if (float.IsNaN(colliderPosition.X) || float.IsNaN(colliderPosition.Y)) {
                // Reset all movement properties
                colliderPosition = Vector2.Zero;
                acceleration = Vector2.Zero;
                velocity = Vector2.Zero;
            }

            // Important to draw shape on every update, because the rotation of the shape is dependant on where the target is
            shape = CreateShape();

            // Create the vision vector
            ahead = colliderPosition + Vector2.Normalize(velocity) * visionLength;
        }

        public new void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            spriteBatch.DrawPolygon(colliderPosition, shape, color);
            
            if(Game1.Debug)
                spriteBatch.DrawLine(colliderPosition, ahead, Color.Red, 2f);

            spriteBatch.End();

            base.Draw(spriteBatch);
        }

        // Move towards the target with smooth turning
        private Vector2 Seek(Vector2 targetPosition) {
            Vector2 direction = Vector2.Normalize(targetPosition - colliderPosition);
            direction *= maxSpeed;
            Vector2 steering = Vector2.Normalize(direction - velocity);
            steering = steering.Truncate(maxForce);

            return steering;
        }

        // Avoid obstacles with smooth turning
        private Vector2 Avoid(List<Obstacle> obstacles) {
            float closestObstacleDistance = 1 / 0f; // Initially, set it to infinity
            Obstacle closestObstacle = null; // Most threatening
            foreach (Obstacle obstacle in obstacles) {
                // Check if ahead line collides with the obstacle
                CircleLineCollisionResult result = new NetRumble.CollisionMath.CircleLineCollisionResult();
                CircleLineCollide(obstacle.colliderPosition, obstacle.colliderRadius, colliderPosition, ahead, ref result);
                if (result.Collision) { // Collision
                    float distanceCreatureToObstacle = Vector2.Distance(colliderPosition, obstacle.colliderPosition);

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

            Vector2 avoidanceForce = Vector2.Normalize(ahead - closestObstacle.colliderPosition);
            avoidanceForce = avoidanceForce.Truncate(maxForce);
            return avoidanceForce;
        }

        // Creates a polygon shape
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
}
