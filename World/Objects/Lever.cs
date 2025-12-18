using GameTemplate;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace MineGame.World.Objects;

public class Lever : WorldObject {
    private Texture2D texture;
    private Rectangle baseASource, baseBSource, armSource, armNodeSource, mouseNodeSource;
    public float Rotation { get; set; } = -MathHelper.PiOver4;
    public float Scale { get; set; } = 1f;

    private Vector2 basePivot, armPivot, armNodePivot;
    private float armLength = 24f;

    public Lever(Texture2D texture, Vector2 position) {
        this.texture = texture;
        Position = position;

        baseASource = new Rectangle(0, 0, 32, 32);
        baseBSource = new Rectangle(32, 0, 32, 32);
        armSource = new Rectangle(64, 0, 32, 32);
        armNodeSource = new Rectangle(96, 0, 32, 32);
        mouseNodeSource = new Rectangle(128, 0, 32, 32);

        basePivot = new Vector2(baseASource.Width / 2f, baseASource.Height / 2f);
        armPivot = new Vector2(0f, armSource.Height / 2f);
        armNodePivot = new Vector2(armSource.Width / 2f, armSource.Height / 2f);
    }

    public static float LerpAngle(float a, float b, float t) {
        float diff = b - a;
        while (diff < -MathHelper.Pi)
            diff += MathHelper.TwoPi;
        while (diff > MathHelper.Pi)
            diff -= MathHelper.TwoPi;
        return a + diff * t;
    }


    private float RotationSpeed = 0f;   // current angular velocity
    private float MaxAngularSpeed = MathHelper.Pi; // rad/sec
    private float AngularAcceleration = MathHelper.Pi; // rad/sec²
    private float Damping = 0.96f; // friction

    static Lever activeLever = null;
    bool isDragging;
    float? targetRotation = null;

    public override void Update(GameTime gameTime) {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var mouse = Mouse.GetState();
        Vector2 mouseWorld = Game1.Instance.Camera.MouseWorld;

        if (mouse.LeftButton == ButtonState.Pressed) {
            if (activeLever == null && GetArmNodeCircle().Contains(mouseWorld)) {
                activeLever = this;
                isDragging = true;
            }
        }

        if (mouse.LeftButton == ButtonState.Released) {
            if (activeLever == this) {
                activeLever = null;
                isDragging = false;
                targetRotation = null; // release, no hard stop
            }
        }

        if (isDragging && activeLever == this) {
            Vector2 dir = mouseWorld - Position;
            if (dir.LengthSquared() > 0.0001f)
                targetRotation = MathF.Atan2(dir.Y, dir.X);
        }

        if (targetRotation.HasValue) {
            float diff = targetRotation.Value - Rotation;
            while (diff < -MathHelper.Pi)
                diff += MathHelper.TwoPi;
            while (diff > MathHelper.Pi)
                diff -= MathHelper.TwoPi;

            RotationSpeed += diff * AngularAcceleration * dt;
        }

        // inertia always runs
        RotationSpeed = MathHelper.Clamp(RotationSpeed, -MaxAngularSpeed, MaxAngularSpeed);
        RotationSpeed *= Damping;
        Rotation += RotationSpeed * dt;

        float momentOfInertia = 1f; // arbitrary scalar for lever
        float rotationalEnergy = 0.5f * momentOfInertia * RotationSpeed * RotationSpeed;
        // Debug.WriteLine($"Rotational energy: {rotationalEnergy}");

        base.Update(gameTime);
    }


    public override void Draw(SpriteBatch spriteBatch) {
        spriteBatch.Draw(
            texture,
            Position,
            baseASource,
            Color.White,
            0f,
            basePivot,
            Scale,
            SpriteEffects.None,
            0f
        );
    }

    public void DrawArm(SpriteBatch spriteBatch) {
        // arm rotates around left edge, but positioned relative to base center
        spriteBatch.Draw(
            texture,
            Position,
            armSource,
            Color.White,
            Rotation,
            armPivot,
            new Vector2(Scale * armLength / armSource.Width, Scale),
            SpriteEffects.None,
            0f
        );

        // base centered at Position
        spriteBatch.Draw(
            texture,
            Position,
            baseBSource,
            Color.White,
            0f,
            basePivot,
            Scale,
            SpriteEffects.None,
            0f
        );
        spriteBatch.Draw(
            texture,
            Position,
            baseBSource,
            Color.White,
            0f,
            basePivot,
            Scale,
            SpriteEffects.None,
            0f
        );

        Vector2 armEnd =
        Position +
        Vector2.Transform(
            new Vector2(armLength * Scale, 0f),
            Matrix.CreateRotationZ(Rotation)
        );
        spriteBatch.Draw(
            texture,
            armEnd,
            armNodeSource,
            Color.White,
            Rotation,
            armNodePivot,
            Scale,
            SpriteEffects.None,
            0f
        );


        /*
        spriteBatch.Draw(
            texture,
            armEnd,
            mouseNodeSource,
            Color.White,
            0f,
            armNodePivot,
            Scale*(1/Game1.Instance.Camera.Zoom),
            SpriteEffects.None,
            0f
        );
        */

        base.Draw(spriteBatch);
    }

    public Rectangle GetBounds() {
        return new Rectangle(
            (int)(Position.X - basePivot.X * Scale),
            (int)(Position.Y - basePivot.Y * Scale),
            (int)(baseASource.Width * Scale),
            (int)(baseASource.Height * Scale)
        );
    }

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

    Circle GetArmNodeCircle() {
        Vector2 armEnd =
            Position +
            Vector2.Transform(
                new Vector2(armLength * Scale, 0f),
                Matrix.CreateRotationZ(Rotation)
            );

        return new Circle(
            armEnd,
            armNodeSource.Width * Scale * 0.2f
        );
    }
}
