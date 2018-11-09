using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace SteeringBehaviors {
    public class Obstacle : Collider {
        public Color color { get; set; }
        private float radius;
        public static readonly float colliderRadiusOffset = 10;

        public Obstacle(Vector2 position, float radius) : base(position, radius + colliderRadiusOffset) {
            color = Color.Green;
            this.radius = radius;
        }

        public new void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            spriteBatch.DrawCircle(colliderPosition, radius, 180, Color.Red);
            spriteBatch.End();

            base.Draw(spriteBatch);
        }
    }
}
