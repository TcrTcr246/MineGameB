using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGameB;
using System;

namespace GameTemplate;
public class Camera2D {
    public Vector2 Position { get; protected set; }
    public float Zoom = 1f;
    public float TotalZoom => Zoom * Letterbox.Scale;
    public float Rotation = 0f;
    public float FollowSpeed = 6f;

    public Rectangle? WorldBounds = null;
    public Rectangle DeadZone = new(40, 40, 40, 40);

    public bool TranslateBackToWorldPos = false;
    public static Rectangle Screen => new(0, 0, Letterbox.VirtualWidth, Letterbox.VirtualHeight);

    Vector2 shake;
    float shakeTimer = 0f;
    float shakeStrength = 0f;

    static readonly Random rng = new();

    public static Vector2 MouseWorld {
        get {
            MouseState ms = Mouse.GetState();
            return WorldToScreenPoint(new(ms.X, ms.Y));
        }
    }

    Rectangle CamRect => new(
        (int)(Position.X - DeadZone.Width / 2),
        (int)(Position.Y - DeadZone.Height / 2),
        DeadZone.Width,
        DeadZone.Height
    );

    MouseState ms, lastMs;
    public Camera2D() {
        lastMs = Mouse.GetState();
        ms = Mouse.GetState();
    }

    public void Update(GameTime gameTime) {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        lastMs = ms;
        ms = Mouse.GetState();

        float halfW = Letterbox.ViewportWidth * 0.5f;
        float halfH = Letterbox.ViewportHeight * 0.5f;

        if (WorldBounds is Rectangle wb) {
            var p = Position;
            p.X = MathHelper.Clamp(p.X, wb.Left + halfW, wb.Right - halfW);
            p.Y = MathHelper.Clamp(p.Y, wb.Top + halfH, wb.Bottom - halfH);
            Position = p;
        }

        shake = new Vector2(0, 0);
        if (shakeTimer > 0) {
            shakeTimer -= dt;
            shake += new Vector2(
                MainRandom(-shakeStrength, shakeStrength),
                MainRandom(-shakeStrength, shakeStrength)
            );
        }

        if (TargetZoom == uniqueFloatCode)
            TargetZoom = Zoom;
        Zoom = MathHelper.Lerp(Zoom, TargetZoom, scrollLerpSpeed * dt);
    }

    public void MoveHardTo(Vector2 playerPos) => Position = playerPos;

    public void MoveTo(GameTime gameTime, Vector2 playerPos) {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 cameraTarget = Position;

        if (playerPos.X < CamRect.Left)
            cameraTarget.X = playerPos.X + DeadZone.Width / 2;

        if (playerPos.X > CamRect.Right)
            cameraTarget.X = playerPos.X - DeadZone.Width / 2;

        if (playerPos.Y < CamRect.Top)
            cameraTarget.Y = playerPos.Y + DeadZone.Height / 2;

        if (playerPos.Y > CamRect.Bottom)
            cameraTarget.Y = playerPos.Y - DeadZone.Height / 2;

        Position = Vector2.Lerp(Position, cameraTarget, FollowSpeed * dt);
    }

    public void MoveIndependent(GameTime gameTime, float speed) {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var k = Keyboard.GetState();

        Vector2 move = Vector2.Zero;

        if (k.IsKeyDown(Keys.W)) move.Y -= 1;
        if (k.IsKeyDown(Keys.S)) move.Y += 1;
        if (k.IsKeyDown(Keys.A)) move.X -= 1;
        if (k.IsKeyDown(Keys.D)) move.X += 1;

        if (move != Vector2.Zero)
            move.Normalize();

        Position += move * speed * dt;
    }

    const float uniqueFloatCode = float.MinValue;
    float TargetZoom = uniqueFloatCode;
    float scrollLerpSpeed = 3f;

    public void ScaleIndependent(GameTime gameTime, float scrollSpeed=100f, float lerpSpeed=7f, float minimum=0.001f, float maximum=2.5f) {
        if (TargetZoom == uniqueFloatCode) // 0.001f 0.45f
            TargetZoom = Zoom;

        int sc = ms.ScrollWheelValue - lastMs.ScrollWheelValue;
        if (sc != 0) {
            float distanceFactor = TargetZoom;

            TargetZoom += sc * (scrollSpeed / 100000f) * distanceFactor;
            TargetZoom = MathHelper.Clamp(TargetZoom, minimum, maximum);
            scrollLerpSpeed = lerpSpeed;
        }
    }

    public Matrix GetTransform(GraphicsDevice __) {
        var letterboxSize = new Vector3(
            Letterbox.ViewportWidth,
            Letterbox.ViewportHeight,
            0f
        );
        var PosMat = new Vector3(-Position, 0);

        return
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateTranslation(PosMat) *
            Matrix.CreateScale(TotalZoom) *
            Matrix.CreateTranslation(letterboxSize / 2);
        // (TranslateBackToWorldPos ? - letterboxSize/2 : Vector3.Zero)
    }

    public static Vector2 WorldToScreenPoint(Vector2 p) {
        Vector2 pct = new Vector2(p.X, p.Y) - new Vector2(Letterbox.OffsetX, Letterbox.OffsetY);
        Matrix view = Game1.Instance.Camera.GetTransform(Game1.Instance.GraphicsDevice);
        Matrix.Invert(ref view, out Matrix inverseView);
        return Vector2.Transform(pct, inverseView);
    }

    public static Matrix GetTransformUnpositioned(GraphicsDevice gd) {
        _ = gd;
        return
            Matrix.CreateScale(Letterbox.Scale);
        // (TranslateBackToWorldPos ? - letterboxSize/2 : Vector3.Zero)
    }

    public Rectangle GetViewBounds(GraphicsDevice gd) {
        Matrix transform = GetTransform(gd);

        // invert matrix: screen -> world
        Matrix.Invert(ref transform, out Matrix inverse);

        // screen corners
        Vector2 topLeft = Vector2.Transform(Vector2.Zero, inverse);
        Vector2 topRight = Vector2.Transform(new Vector2(Letterbox.ViewportWidth, 0), inverse);
        Vector2 bottomLeft = Vector2.Transform(new Vector2(0, Letterbox.ViewportHeight), inverse);
        Vector2 bottomRight = Vector2.Transform(new Vector2(Letterbox.ViewportWidth, Letterbox.ViewportHeight), inverse);

        // min/max x
        float minX = MathF.Min(MathF.Min(topLeft.X, topRight.X), MathF.Min(bottomLeft.X, bottomRight.X));
        float maxX = MathF.Max(MathF.Max(topLeft.X, topRight.X), MathF.Max(bottomLeft.X, bottomRight.X));

        // min/max y
        float minY = MathF.Min(MathF.Min(topLeft.Y, topRight.Y), MathF.Min(bottomLeft.Y, bottomRight.Y));
        float maxY = MathF.Max(MathF.Max(topLeft.Y, topRight.Y), MathF.Max(bottomLeft.Y, bottomRight.Y));

        return new Rectangle(
            (int)minX,
            (int)minY,
            (int)(maxX - minX),
            (int)(maxY - minY)
        );
    }


    public void Shake(float strength, float time) {
        shakeStrength = strength;
        shakeTimer = time;
    }

    private static float MainRandom(float min, float max) {
        return (float)(rng.NextDouble() * (max - min) + min);
    }
}
