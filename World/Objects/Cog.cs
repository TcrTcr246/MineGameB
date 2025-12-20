using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MineGameB.World.Objects;

public class Cog : WorldObject {
    private readonly Texture2D texture;
    private readonly Rectangle source = new(96, 0, 48, 48);
    private readonly Vector2 pivot = new(24f, 24f);

    public float Rotation { get; set; } = 0f;
    public float RotationSpeed { get; set; } = 0f;
    public float Scale { get; set; } = 1f;

    static int count = 0;
    public Cog(Texture2D texture, Vector2 position) {
        RotationSpeed = count%2*2-1f;
        this.texture = texture;
        Position = position;
        count++;
    }

    public override void Update(GameTime gameTime) {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Rotation += RotationSpeed * dt;
        base.Update(gameTime);
    }

    public override void DrawLayer(SpriteBatch spriteBatch, int layer) {
        if (layer == -4) {
            spriteBatch.Draw(
                texture,
                Position,
                source,
                Color.White,
                Rotation,
                pivot,
                Scale,
                SpriteEffects.None,
                0f
            );
        }
    }

    public Rectangle GetBounds() {
        return new Rectangle(
            (int)(Position.X - pivot.X * Scale),
            (int)(Position.Y - pivot.Y * Scale),
            (int)(source.Width * Scale),
            (int)(source.Height * Scale)
        );
    }
}
