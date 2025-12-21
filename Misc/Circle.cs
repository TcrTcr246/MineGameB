using Microsoft.Xna.Framework;

namespace MineGameB.Misc;
public class Circle {
    public Vector2 Center;
    public float Radius;

    public Circle(Vector2 center, float radius) {
        Center = center;
        Radius = radius;
    }

    public bool Contains(Vector2 p) {
        return Vector2.DistanceSquared(p, Center) <= Radius * Radius;
    }
}
