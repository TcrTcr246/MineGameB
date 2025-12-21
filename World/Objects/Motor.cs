using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MineGameB.World.Objects;

public class Motor : WorldObject {
    protected float RotationForce = MathHelper.PiOver2/2f;
    public Motor SetRotationForce(float force) {
        RotationForce = force;
        return this;
    }

    // Optimization: accumulate rotations and batch apply
    private const float MinRotationDelta = 0.01f; // ~0.57 degrees
    private float accumulatedRotation = 0f;

    protected Texture2D texture;
    protected Rectangle source;
    protected Vector2 basePivot;
    protected Vector2 Scale = new(1f, 1f);
    protected Cog cogweel = null;

    public Motor(Texture2D texture) : base() {
        source = new Rectangle(64, 0, 32, 32);
        this.texture = texture;
        basePivot = new Vector2(source.Width / 2f, source.Height / 2f);
    }

    public override void OnSetMapPosition(Point pos) {
        cogweel = Lever.GetCog(MapRef, pos);
    }


    public override void DrawLayer(SpriteBatch spriteBatch, int layer) {
        switch (layer) {
            case 3:
            spriteBatch.Draw(
                texture,
                Position,
                source,
                Color.White,
                0f,
                basePivot,
                Scale,
                SpriteEffects.None,
                0f
            );
            break;
        }
    }

    public override void Update(GameTime gameTime) {
        if (cogweel == null)
            return;

        float rotationDelta = RotationForce * (float)gameTime.ElapsedGameTime.TotalSeconds;
        accumulatedRotation += rotationDelta;

        // Only apply rotation when accumulated amount is significant
        if (Math.Abs(accumulatedRotation) >= MinRotationDelta) {
            cogweel.ApplyRotation(accumulatedRotation);
            accumulatedRotation = 0f;
        }
    }
}