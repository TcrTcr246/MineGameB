using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MineGameB.World.Objects;

public class BigCog : Cog {
    // Cache for diagonal cogs (BigCog connects diagonally to regular Cogs)
    private List<Cog> cachedDiagonalCogs;
    private bool diagonalCacheDirty = true;

    // Static array to avoid allocation
    private static readonly Point[] diagonalDirs = [
        new(-1, 1), new(1, 1),
        new(-1, -1), new(1, -1)
    ];

    public BigCog(Texture2D texture) : base(texture) {
        source = new(0, 48, 64, 64);
        pivot = new(32f, 32f);
        Teet = 12;
        CogRotateMultiplier = 1f;
    }

    public override void AddRatioOnLoad(int x, int y) {
        Rotation -= MathHelper.ToRadians(10f);
        if ((x + y) % 2 == 1)
            Rotation += MathHelper.ToRadians(180f / (Teet * 2));
    }

    // Override to also invalidate BigCog's specific cache
    public new void InvalidateCache() {
        base.InvalidateCache();
        diagonalCacheDirty = true;
    }

    private void RebuildDiagonalCache() {
        if (!diagonalCacheDirty)
            return;

        cachedDiagonalCogs = new List<Cog>(4);

        foreach (var d in diagonalDirs) {
            var list = MapRef.GetObjectListAtPos(TilePosition + d);
            if (list is null)
                continue;

            if (Map.FindObject(list, o => o is Cog) is Cog gear)
                cachedDiagonalCogs.Add(gear);
        }

        diagonalCacheDirty = false;
    }

    protected new void RotateAroundGears(float movement, HashSet<Cog> visited) {
        RebuildDiagonalCache();

        for (int i = 0; i < cachedDiagonalCogs.Count; i++) {
            var gear = cachedDiagonalCogs[i];
            gear?.ApplyRotation(-movement * Teet / gear.Teet, visited);
        }
    }

    public override void OnRotationAplication(float movement, HashSet<Cog> visited) {
        RotateAroundGears(movement, visited);
        // Note: BigCog doesn't call RotateAroundBigGears from base
    }
}