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
        private readonly float maxForce = 0.18f;
        private readonly float maxSpeed = 0.1f;//5f; // Clamped at 1.5f

        // Visual properties
        private int width;
        private int height;
        private Polygon shape;
        private float rotation;
        private Color color;

        // Avoidance properties
        private Vector2 ahead;
        private readonly float visionLength = 50f;
        private readonly Angle visionAngle = new Angle((float)Math.PI / 4);

        private bool debug;

        public Creature(Vector2 position, int size, Color color, SpriteBatch spriteBatch, bool debug = false) : base(position, size, spriteBatch) {
            colliderPosition = position;
            width = size;
            height = size + 5;
            this.color = color;
            this.debug = debug;
            shape = CreateShape();
        }

        public void Update(Target target, List<Collider> colliders) {
            if (debug) {
                color = Color.Blue;
            }

            List<Vector2> visibles = Vision(colliders);

            // Keep tracking the target
            Vector2 seekForce = Seek(target.colliderPosition);

            // Avoid any obstacles
            Vector2 avoidForce = Avoid(visibles);

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
        }

        public new void Draw() {
            // Translate to world origin, rotate, translate back to our world position
            // this enables us to define al our vectors with respect to the local origin (our position)
            Matrix rotationMatrix = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(new Vector3(colliderPosition.X, colliderPosition.Y, 0));

            spriteBatch.Begin(transformMatrix: rotationMatrix);
            spriteBatch.DrawPolygon(Vector2.Zero, shape, color);

            if (Game1.Debug) {
                spriteBatch.DrawLine(Vector2.Zero, new Vector2(0, visionLength).Rotate(visionAngle), Color.LightBlue);
                spriteBatch.DrawLine(Vector2.Zero, new Vector2(0, visionLength).Rotate(-visionAngle), Color.LightBlue);
            }
            spriteBatch.End();

            // ahead already has the right rotation
            spriteBatch.Begin();
            if (Game1.Debug) {
                //
            }
            spriteBatch.End();

            base.Draw();
        }

        private List<Vector2> Vision(List<Collider> colliders) {
            List<Vector2> visibleVectors = new List<Vector2>();

            // Create the vision vector
            ahead = colliderPosition + Vector2.Normalize(velocity) * visionLength;

            foreach (Collider collider in colliders) {
                if (collider.Equals(this)) {
                    continue; // You shouldn't be able to 'see' yourself
                }

                DrawLater(() => spriteBatch.DrawPoint(ahead, Color.Red, 5f)); // DEBUG: How far can we see in front of us (old)

                DrawLater(() => spriteBatch.DrawCircle(colliderPosition, visionLength, 180, Color.DarkBlue)); // DEBUG: How far we can see around us
                if (!CircleCircleIntersect(colliderPosition, visionLength, collider.colliderPosition, collider.colliderRadius)) {
                    continue; // Collider is not in range of our vision 'collider'
                }

                // Check if a line from here to collider intersects with the collider
                // TODO: Get the point (on the edge) where it collides, so we can steer away from that point
                // instead of steering away from the collider's center
                // I thought result.point would help, but this one just sits at the collider's center
                // BTW i disabled Avoid(), so it doesn't start spinning when it sees something
                CircleLineCollisionResult result = new CircleLineCollisionResult();
                CircleLineCollide(collider.colliderPosition, collider.colliderRadius, colliderPosition, collider.colliderPosition, ref result);

                DrawLater(() => spriteBatch.DrawLine(colliderPosition, result.Point, Color.Orange)); // DEBUG: Should draw a line to the intersection point on the edge of the circle, but instead it draws it on the center
                DrawLater(() => spriteBatch.DrawPoint(result.Point, result.Collision ? Color.LightGreen : Color.LightBlue, 10f)); // DEBUG: Intersection point between our ray and the collider

                if (!result.Collision) {
                    continue; // Collider is not in range
                }

                // Check if the collider is in our vision segment
                Vector2 aheadPos = ahead - colliderPosition; // ahead with respect to our position
                Vector2 colliderPos = result.Point - colliderPosition;

                double ourMagnitude = Math.Sqrt(Math.Pow(aheadPos.X, 2) + Math.Pow(aheadPos.Y, 2));
                double theirMagnitude = Math.Sqrt(Math.Pow(colliderPos.X, 2) + Math.Pow(colliderPos.Y, 2));
                double angleBetweenUs = Math.Acos(aheadPos.Dot(colliderPos) / (ourMagnitude * theirMagnitude));
                if (Double.IsNaN(angleBetweenUs))
                    angleBetweenUs = 0;

                if (angleBetweenUs < -visionAngle || angleBetweenUs > visionAngle) {
                    continue; // Target is not within vision angles
                }

                if (theirMagnitude > ourMagnitude) {
                    continue; // Target is not within vision length         
                }

                DrawLater(() => spriteBatch.DrawPoint(collider.colliderPosition, Color.Green, 5f)); // DEBUG: What we see is in range

                visibleVectors.Add(ahead);
            }

            return visibleVectors;
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
        private Vector2 Avoid(List<Vector2> points) {
            return Vector2.Zero; // DEBUG

            float closesPointDistance = 1 / 0f; // Initially, set it to infinity
            Vector2 closesPoint = Vector2.Zero; // Most threatening
            foreach (Vector2 point in points) {
                float distanceCreatureToPoint = Vector2.Distance(colliderPosition, point);

                // Check if this is the closest obstacle with respect to the creature's position
                if (distanceCreatureToPoint < closesPointDistance) {
                    closesPointDistance = distanceCreatureToPoint;
                    closesPoint = point;
                }
            }

            if (closesPoint == Vector2.Zero) {
                return Vector2.Zero;
            }

            DrawLater(() => spriteBatch.DrawLine(colliderPosition, closesPoint, Color.Purple));

            Vector2 avoidanceForce = Vector2.Normalize(ahead - closesPoint);
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
