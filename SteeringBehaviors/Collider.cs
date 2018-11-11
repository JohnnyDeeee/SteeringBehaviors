using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace SteeringBehaviors {
    public class Collider {
        public Vector2 colliderPosition { get; set; }
        public float colliderRadius { get; set; }
        public SpriteBatch spriteBatch { get; private set; }
        protected List<Action> spriteBatchBuffer = new List<Action>();

        public Collider(Vector2 position, float radius, SpriteBatch spriteBatch) {
            colliderPosition = position;
            colliderRadius = radius;
            this.spriteBatch = spriteBatch;
        }

        protected void Draw() {
            if (!Game1.Debug) {
                spriteBatchBuffer.Clear();
                return;
            }

            spriteBatch.Begin();
            spriteBatch.DrawCircle(colliderPosition, colliderRadius, 180, Color.White);
            spriteBatchBuffer.ForEach(x => x());
            spriteBatch.End();

            spriteBatchBuffer.Clear();
        }

        protected void Update() { }

        protected void DrawLater(Action action) {
            spriteBatchBuffer.Add(action);
        }
    }
}
