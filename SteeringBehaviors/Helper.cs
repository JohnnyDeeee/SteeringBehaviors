using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteeringBehaviors {
    public static class Helper {
        public static Vector2 RotateAroundOrigin(Vector2 point, Vector2 origin, float rotation) {
            return Vector2.Transform(point - origin, Matrix.CreateRotationZ(rotation)) + origin;
        }
    }
}
