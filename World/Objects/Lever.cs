using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MineGameB.World.Objects;

public class Lever : WorldObject {
    private Texture2D texture;
    protected Rectangle baseASource, baseBSource, armSource, armNodeSource;
    public float Rotation { get; set; } = -MathHelper.PiOver4;
    public float LastRotation { get; set; }
    public float Scale { get; set; } = 1f;

    private Vector2 basePivot, armPivot, armNodePivot;
    private float armLength = 24f;

    public Cog cogweel = null;

    public Lever(Texture2D texture, Cog cogweel=null) : base() {
        this.texture = texture;
        LastRotation = Rotation;
        if (cogweel is not null)
            this.cogweel = cogweel;

        baseASource = new Rectangle(0, 0, 32, 32);
        baseBSource = new Rectangle(32, 0, 32, 32);
        armSource = new Rectangle(0, 32, 16, 16);
        armNodeSource = new Rectangle(16, 32, 16, 16);

        basePivot = new Vector2(baseASource.Width / 2f, baseASource.Height / 2f);
        armPivot = new Vector2(0f, armSource.Height / 2f);
        armNodePivot = new Vector2(armSource.Width / 2f, armSource.Height / 2f);
        Load();
    }

    public override void OnSetMapPosition(Point pos) {
        cogweel ??= (Cog)Map.FindObject(MapRef.GetObjectListAtPos(pos), cog => cog is Cog);
    }

    public static float LerpAngle(float a, float b, float t) {
        float diff = b - a;
        while (diff < -MathHelper.Pi)
            diff += MathHelper.TwoPi;
        while (diff > MathHelper.Pi)
            diff -= MathHelper.TwoPi;
        return a + diff * t;
    }


    private float RotationSpeed = 0f;
    private float MaxAngularSpeed = MathHelper.Pi/3;
    private float AngularAcceleration = MathHelper.Pi*1.5f;
    private float Damping = 0.98f;

    static Lever activeLever = null;
    bool isDragging;
    float? targetRotation = null;
    public float RotationalEnergy { private set; get; } = 0f;
    protected bool cogAttached = true;

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

        RotationSpeed = MathHelper.Clamp(RotationSpeed, -MaxAngularSpeed, MaxAngularSpeed);
        RotationSpeed *= Damping;

        Rotate(RotationSpeed * dt);

        float momentOfInertia = 1f;
        RotationalEnergy = 0.5f * momentOfInertia * RotationSpeed * RotationSpeed;

        base.Update(gameTime);
    }

    public void Rotate(float value, float dt = 1) {
        LastRotation = Rotation;
        Rotation += value * dt;
        cogweel.ApplyRotation((Rotation - LastRotation) * cogweel.CogRotateMultiplier, null, true);
    }
    public void RotateFromCog(float value, float dt = 1) {
        Rotation += (value * dt) / cogweel.CogRotateMultiplier;
        LastRotation = Rotation;
    }

    protected void DrawBaseA(SpriteBatch spriteBatch) {
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

    public override void DrawLayer(SpriteBatch spriteBatch, int layer) {
        switch(layer) {
            case -2:
                if (!cogAttached)
                    DrawBaseA(spriteBatch);
                break;
            case 0:
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

                if (!cogAttached) {
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
                }

                spriteBatch.Draw(
                    texture,
                    Position,
                    baseBSource,
                    Color.White,
                    Rotation,
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
            break;
        }
    }

    public override Rectangle GetBounds() {
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
