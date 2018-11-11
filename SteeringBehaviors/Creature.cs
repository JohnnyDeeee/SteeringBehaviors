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
        private readonly float maxForce = 0.48f;
        private readonly float maxSpeed = 5f;

        // Visual properties
        private int width;
        private int height;
        private Polygon shape;
        private float rotation;
        private Color color;

        // Avoidance properties
        private Vector2 ahead;
        private readonly float visionLength = 90f;
        private readonly Angle visionAngle = new Angle((float)Math.PI / 4);

        public Creature(Vector2 position, int size, Color color, SpriteBatch spriteBatch) : base(position, size, spriteBatch) {
            colliderPosition = position;
            width = size;
            height = size + 5;
            this.color = color;
            shape = CreateShape();
        }

        public void Update(Target target, List<Collider> colliders) {
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
                //
            }
            spriteBatch.End();

            base.Draw();
        }

        // Detects any vector that is withing the vision range and returns it as a list
        private List<Vector2> Vision(List<Collider> colliders) {
            List<Vector2> visibleVectors = new List<Vector2>();

            // Create the vision vector
            ahead = colliderPosition + Vector2.Normalize(velocity) * visionLength;

            // Create an arc of 'detector points'
            int degrees = (int)MathHelper.ToDegrees(visionAngle);
            int stepSize = 15;
            List<Vector2> detectorPoints = new List<Vector2>();
            for (int i = -degrees; i < degrees; i+=stepSize) {
                Vector2 point = Helper.RotateAroundOrigin(ahead, colliderPosition, MathHelper.ToRadians(i));
                detectorPoints.Add(point);
                DrawLater(() => spriteBatch.DrawPoint(point, Color.LightBlue, 5f));
            }

            // Go through every collider in this world
            foreach (Collider collider in colliders) {
                if (collider.Equals(this)) {
                    continue; // You shouldn't be able to 'see' yourself
                }

                DrawLater(() => spriteBatch.DrawCircle(colliderPosition, visionLength, 180, Color.DarkBlue)); // DEBUG: How far we can see around us
                if (!CircleCircleIntersect(colliderPosition, visionLength, collider.colliderPosition, collider.colliderRadius)) {
                    continue; // Collider is not in range of our vision 'collider'
                }

                // Check if a detector point intersects with the collider
                // If so, we can add this detector point to the list of visible vectors
                // TODO: It is not really a visible vector, you'd expect the collider to get added to the list... Maybe refactor this..
                foreach(Vector2 detector in detectorPoints) {
                    DrawLater(() => spriteBatch.DrawLine(colliderPosition, detector, Color.LightBlue)); // DEBUG: Draw a line to all detector points
                    CircleLineCollisionResult result = new CircleLineCollisionResult();
                    CircleLineCollide(collider.colliderPosition, collider.colliderRadius, colliderPosition, detector, ref result);
                    if (result.Collision) {
                        DrawLater(() => spriteBatch.DrawPoint(detector, Color.Red, 5f)); // DEBUG: Show the detector point as red if he detects something
                        visibleVectors.Add(detector);
                    }
                }
            }

            return visibleVectors;
        }

        // Move towards the target with smooth turning
        private Vector2 Seek(Vector2 targetPosition) {
            Vector2 direction = Vector2.Normalize(targetPosition - colliderPosition);
            direction *= maxSpeed;
            Vector2 steering = Vector2.Normalize(direction - velocity);
            steering = steering.Truncate(maxForce);

            return steering;
        }

        // Avoid points with smooth turning
        private Vector2 Avoid(List<Vector2> points) {
            float closesPointDistance = 1 / 0f; // Initially, set it to infinity
            Vector2 closestPoint = Vector2.Zero; // Most threatening
            
            if (points.Count > 1) { // Don't need to decide which point is the most threatening if there is only 1 point
                foreach (Vector2 point in points) {
                    float distanceCreatureToPoint = Vector2.Distance(colliderPosition, point);

                    // Check if this is the closest obstacle with respect to the creature's position
                    if (distanceCreatureToPoint < closesPointDistance) {
                        closesPointDistance = distanceCreatureToPoint;
                        closestPoint = point;
                    }
                }
            } else if(points.Count == 1) {
                closestPoint = points[0];
            }

            if (closestPoint == Vector2.Zero) {
                return Vector2.Zero;
            }

            DrawLater(() => spriteBatch.DrawPoint(closestPoint, Color.Purple, 10f)); // DEBUG: Draw the point that we are going to avoid

            Vector2 avoidanceForce = Vector2.Normalize(ahead - closestPoint);
            avoidanceForce = avoidanceForce.Truncate(maxForce);

            // TODO: Fix issue where avoidanceForce = NaN
            if (Double.IsNaN(avoidanceForce.X) || Double.IsNaN(avoidanceForce.Y))
                return Vector2.Zero;

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
