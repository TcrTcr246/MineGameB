using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace MineGameB.World.Objects;

public class Cog : WorldObject {
    private readonly Texture2D texture;
    private readonly Rectangle source = new(96, 0, 48, 48);
    private readonly Vector2 pivot = new(24f, 24f);

    public float Rotation { get; set; } = 0f;
    public float RotationSpeed { get; set; } = 0f;
    public float Scale { get; set; } = 1f;
    public float CogRotateMultiplier { get; set; } = 0.75f;

    public Cog(Texture2D texture) {
        // RotationSpeed = count%2*2-1f;
        this.texture = texture;
    }

    public void ApplyRotation(float movement, HashSet<Cog> visited = null, bool fromLever = false) {
        visited ??= [];
        if (!visited.Add(this))
            return;

        Rotation += movement;

        Point[] dirs = [
            new(0, 1), new(0, -1),
            new(1, 0), new(-1, 0)
        ];

        var listA = MapRef.GetObjectListAtPos(TilePosition);
        if (!fromLever && listA is not null) {
            var lever = (Lever)Map.FindObject(listA, o => o is Lever);
            lever?.RotateFromCog(movement);
        }

        foreach (var d in dirs) {
            var list = MapRef.GetObjectListAtPos(TilePosition + d);
            if (list is null)
                continue;

            var cog = (Cog)Map.FindObject(list, o => o is Cog);
            cog?.ApplyRotation(-movement, visited);
        }
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
