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

    // Cache for adjacent cogs to avoid repeated lookups
    private List<Cog> cachedAdjacentCogs;
    private List<BigCog> cachedAdjacentBigCogs;
    private Lever cachedLever;
    private bool cacheDirty = true;
    protected bool Active = true;
    public void SetActive(bool active) => Active = active;
    public bool IsActive() => Active;
    public bool ToggleActive() => Active = !Active;

    // Static arrays to avoid allocation
    private static readonly Point[] orthogonalDirs = [
        new(0, 1), new(0, -1),
        new(1, 0), new(-1, 0)
    ];

    private static readonly Point[] diagonalDirs = [
        new(-1, 1), new(1, 1),
        new(-1, -1), new(1, -1)
    ];

    public float Rotation { get; protected set; } = 0f;
    public void SetRotation(float rotation) => Rotation = rotation;
    public float RotationSpeed { get; protected set; } = 0f;
    public float Scale { get; protected set; } = 1f;
    public float CogRotateMultiplier { get; protected set; }
    public int Teet { get; protected set; }

    public Cog(Texture2D texture) : base() {
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
        cacheDirty = true;
        return base.Load();
    }

    public virtual void AddRatioOnLoad(int x, int y) {
        if ((x + y) % 2 == 0)
            Rotation += MathHelper.ToRadians(180f / (Teet * 2));
    }

    // Call this when gears are added/removed/moved
    public void InvalidateCache() {
        cacheDirty = true;
    }

    // Call this on all gears in an area when the configuration changes
    public static void InvalidateAreaCache(Map map, Point center, int radius = 2) {
        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                var list = map.GetObjectListAtPos(center + new Point(x, y));
                if (list != null) {
                    var cog = Map.FindObject(list, o => o is Cog) as Cog;
                    cog?.InvalidateCache();
                }
            }
        }
    }

    private void RebuildCache() {
        if (!cacheDirty)
            return;

        cachedAdjacentCogs = new List<Cog>(4);
        cachedAdjacentBigCogs = new List<BigCog>(4);
        cachedLever = null;

        // Cache orthogonal cogs
        foreach (var d in orthogonalDirs) {
            var list = MapRef.GetObjectListAtPos(TilePosition + d);
            if (list is null)
                continue;

            if (Map.FindObject(list, o => o is Cog) is Cog cog)
                cachedAdjacentCogs.Add(cog);
        }

        // Cache diagonal big cogs
        foreach (var d in diagonalDirs) {
            var list = MapRef.GetObjectListAtPos(TilePosition + d);
            if (list is null)
                continue;

            if (Map.FindObject(list, o => o is BigCog) is BigCog bigCog)
                cachedAdjacentBigCogs.Add(bigCog);
        }

        // Cache lever at current position
        var listA = MapRef.GetObjectListAtPos(TilePosition);
        if (listA != null) {
            cachedLever = Map.FindObject(listA, o => o is Lever) as Lever;
        }

        cacheDirty = false;
    }

    protected void RotateAroundGears(float movement, HashSet<Cog> visited) {
        RebuildCache();

        for (int i = 0; i < cachedAdjacentCogs.Count; i++) {
            cachedAdjacentCogs[i]?.ApplyRotation(-movement, visited);
        }
    }

    protected void RotateAroundBigGears(float movement, HashSet<Cog> visited) {
        RebuildCache();

        for (int i = 0; i < cachedAdjacentBigCogs.Count; i++) {
            var gear = cachedAdjacentBigCogs[i];
            gear?.ApplyRotation(-movement * Teet / gear.Teet, visited);
        }
    }

    public void ApplyRotation(float movement, HashSet<Cog> visited = null, bool fromLever = false) {
        if (!Active)
            return;

        visited ??= [];

        if (!visited.Add(this))
            return;

        if (!fromLever) {
            RebuildCache();
            cachedLever?.RotateFromCog(movement);
        }

        Rotation += movement;
        OnRotationAplication(movement, visited);
        Rotation = MathHelper.WrapAngle(Rotation);
    }

    public virtual void OnRotationAplication(float movement, HashSet<Cog> visited) {
        RotateAroundGears(movement, visited);
        RotateAroundBigGears(movement, visited);
    }

    public override void Update(GameTime gameTime) {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Rotation += RotationSpeed * dt;
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch) {
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

    public override Rectangle GetBounds() {
        return new Rectangle(
            (int)(Position.X - pivot.X * Scale),
            (int)(Position.Y - pivot.Y * Scale),
            (int)(source.Width * Scale),
            (int)(source.Height * Scale)
        );
    }
}