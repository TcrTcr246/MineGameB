using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;

namespace MineGameB.World.Objects;

public class Cog : WorldObject {
    private readonly Texture2D texture;
    protected Rectangle source;
    protected Vector2 pivot;

    public float Rotation { get; protected set; } = 0f;
    public float RotationSpeed { get; protected set; } = 0f;
    public float Scale { get; protected set; } = 1f;
    public float CogRotateMultiplier { get; protected set; }
    public int Teet { get; protected set; }

    public Cog(Texture2D texture) : base() {
        // RotationSpeed = count%2*2-1f;
        source = new(96, 0, 48, 48);
        pivot = new(24f, 24f);
        Teet = 8;
        CogRotateMultiplier = 1f;
        this.texture = texture;
        Load();
    }

    public override WorldObject Load() {
        int x = TilePosition.X, y = TilePosition.Y;
        AddRatioOnLoad(x, y);
        return base.Load();
    }

    public virtual void AddRatioOnLoad(int x, int y) {
        if ((x + y) % 2 == 0)
            Rotation += MathHelper.ToRadians(180f / (Teet * 2));
    }

    protected void RotateAroundGears(float movement, HashSet<Cog> visited = null) {
        Point[] dirs = [
            new(0, 1), new(0, -1),
            new(1, 0), new(-1, 0)
        ];

        foreach (var d in dirs) {
            var list = MapRef.GetObjectListAtPos(TilePosition + d);
            if (list is null)
                continue;

            ((Cog)Map.FindObject(list, o => o is Cog))?.ApplyRotation(-movement, visited);
        }
    }

    protected void RotateAroundBigGears(float movement, HashSet<Cog> visited = null) {
        Point[] dirs = [
            new(-1, 1), new(1, 1),
            new(-1, -1), new(1, -1)
        ];

        foreach (var d in dirs) {
            var list = MapRef.GetObjectListAtPos(TilePosition + d);
            if (list is null)
                continue;

            var gear = ((BigCog)Map.FindObject(list, o => o is BigCog));
            gear?.ApplyRotation(-movement*Teet/gear.Teet, visited);
        }
    }

    public void ApplyRotation(float movement, HashSet<Cog> visited = null, bool fromLever = false) {
        visited ??= [];
        if (!visited.Add(this))
            return;

        var listA = MapRef.GetObjectListAtPos(TilePosition);
        if (!fromLever && listA is not null) {
            var lever = (Lever)Map.FindObject(listA, o => o is Lever);
            lever?.RotateFromCog(movement);
        }

        Rotation += movement;

        OnRotationAplication(movement, visited);

        Rotation = MathHelper.WrapAngle(Rotation);
    }

    public virtual void OnRotationAplication(float movement, HashSet<Cog> visited = null) {
        RotateAroundGears(movement, visited);
        RotateAroundBigGears(movement, visited);
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

    public override Rectangle GetBounds() {
        return new Rectangle(
            (int)(Position.X - pivot.X * Scale),
            (int)(Position.Y - pivot.Y * Scale),
            (int)(source.Width * Scale),
            (int)(source.Height * Scale)
        );
    }
}
