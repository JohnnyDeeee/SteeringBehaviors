using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using static NetRumble.CollisionMath;

namespace SteeringBehaviors {
    public class Target : Collider {

        public Target(Vector2 position, SpriteBatch spriteBatch) : base(position, 10f, spriteBatch) { }

        public void Update(List<Creature> creatures) {
            // Check for collision with creature
            Collision(creatures);
        }

        public new void Draw() {
            spriteBatch.Begin();
            spriteBatch.DrawPoint(colliderPosition, Color.Red, 10f);
            spriteBatch.End();

            base.Draw();
        }

        private void Collision(List<Creature> creatures) {
            // On collision with a creature, find a random new spot to sit at
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
