using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using NetRumble;
using System;
using System.Collections.Generic;
using static NetRumble.CollisionMath;

namespace SteeringBehaviors {
    public class Creature : Collider {
        // Movement properties
        private Vector2 velocity = Vector2.Zero;
        private Vector2 acceleration = Vector2.Zero;
        private readonly float maxForce = 0.18f;
        private readonly float maxSpeed = 5f; // Clamped at 1.5f

        // Visual properties
        private int width;
        private int height;
        private Polygon shape;
        private float rotation;
        private Color color;
        private Color originalColor;
        public bool marked { get; set; }

        // Avoidance properties
        private Vector2 ahead;
        private readonly float visionLength = 90f;
        private readonly Angle visionAngle = new Angle((float)Math.PI / 4);

        public Creature(Vector2 position, int size, Color color) : base(position, size) {
            colliderPosition = position;
            width = size;
            height = size + 5;
            originalColor = color;
            this.color = originalColor;
            shape = CreateShape();
        }

        public void Update(Target target, List<Obstacle> obstacles, List<Creature> creatures) {
            marked = false; // Always reset our marked state, so that we don't stay marked forever

            Vision(creatures);
            // Create the vision vector
            ahead = colliderPosition + Vector2.Normalize(velocity) * visionLength;

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

            // Rotation
            Vector2 newColliderPosition = colliderPosition + velocity;
            rotation = (float)(Math.Atan2(colliderPosition.Y - newColliderPosition.Y, colliderPosition.X - newColliderPosition.X) + Math.PI / 2); // Rotation towards the velocity

            // Change color if we are marked
            color = marked ? Color.Red : originalColor;
        }

        public new void Draw(SpriteBatch spriteBatch) {
            // Translate to world origin, rotate, translate back to our world position
            // this enables us to define al our vectors with respect to the local origin (our position)
            Matrix rotationMatrix = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(new Vector3(colliderPosition.X, colliderPosition.Y, 0));

            spriteBatch.Begin(transformMatrix: rotationMatrix);
            spriteBatch.DrawPolygon(Vector2.Zero, shape, color);
            spriteBatch.DrawLine(Vector2.Zero, new Vector2(0, visionLength).Rotate(visionAngle), Color.LightBlue);
            spriteBatch.DrawLine(Vector2.Zero, new Vector2(0, visionLength).Rotate(-visionAngle), Color.LightBlue);
            spriteBatch.End();

            // ahead already has the right rotation
            spriteBatch.Begin();
            if (Game1.Debug) {
                spriteBatch.DrawLine(colliderPosition, ahead, Color.Red, 2f);
            }
            spriteBatch.End();

            base.Draw(spriteBatch);
        }

        private void Vision(List<Creature> creatures) {
            // TODO: Get creatures in range (let's say visionLength all around)
            foreach(Creature creature in creatures) {
                if (Vector2.Distance(colliderPosition, creature.colliderPosition) > visionLength)
                    continue; // Ignore creatures that are further than our vision length

                // TODO: Check if angle of our vector to creature vector is within our vision range
                // - if so: creature.marked = true; // Target is in our vision field
                // - else: do nothing
            }
        }

        // Move towards the target with smooth turning
        private Vector2 Seek(Vector2 targetPosition) {
            Vector2 direction = Vector2.Normalize(targetPosition - colliderPosition);
            direction *= MathHelper.Clamp(maxSpeed, 1.5f, maxSpeed); // Must be 1.5f min, otherwise creature is going really fast left and right
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
            Vector2 pointLeft = new Vector2(-width, -width); // Left point of the triangle
            Vector2 pointRight = new Vector2(width, -width); // Right point of the triangle
            Vector2 pointTop = new Vector2(0, height); // Top point of the triangle

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
