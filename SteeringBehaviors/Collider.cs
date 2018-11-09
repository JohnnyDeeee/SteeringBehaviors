using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace SteeringBehaviors {
    public class Collider {
        public Vector2 colliderPosition { get; set; }
        public float colliderRadius { get; set; }

        public Collider(Vector2 position, float radius) {
            this.colliderPosition = position;
            this.colliderRadius = radius;
        }

        protected void Draw(SpriteBatch spriteBatch) {
            if (!Game1.Debug)
                return;

            spriteBatch.Begin();
            spriteBatch.DrawCircle(colliderPosition, colliderRadius, 180, Color.White);
            spriteBatch.End();
        }

        protected void Update() { }
    }
}
