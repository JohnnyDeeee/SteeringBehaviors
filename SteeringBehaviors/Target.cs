using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using static NetRumble.CollisionMath;

namespace SteeringBehaviors {
    public class Target : Collider {

        public Target(Vector2 position) : base(position, 10f) {
            //
        }

        public void Update(List<Creature> creatures) {
            // Check for collision with creature
            Collision(creatures);
        }

        public new void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            spriteBatch.DrawPoint(colliderPosition, Color.Red, 10f);
            spriteBatch.End();

            base.Draw(spriteBatch);
        }

        private void Collision(List<Creature> creatures) {
            Random random = new Random();
            foreach (Creature creature in creatures) {
                if (CircleCircleIntersect(colliderPosition, colliderRadius, creature.colliderPosition, creature.colliderRadius)) {
                    colliderPosition = new Vector2(random.Next(Game1.graphics.PreferredBackBufferWidth), random.Next(Game1.graphics.PreferredBackBufferHeight));
                    break;
                }
            }
        }
    }
}
