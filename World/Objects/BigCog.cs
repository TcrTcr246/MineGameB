using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MineGameB.World.Objects;

public class BigCog : Cog {
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

    protected new void RotateAroundGears(float movement, HashSet<Cog> visited = null) {
        Point[] dirs = [
            new(-1, 1), new(1, 1),
            new(-1, -1), new(1, -1)
        ];

        foreach (var d in dirs) {
            var list = MapRef.GetObjectListAtPos(TilePosition + d);
            if (list is null)
                continue;

            var gear = (Cog)Map.FindObject(list, o => o is Cog);
            gear?.ApplyRotation(-movement * Teet / gear.Teet, visited);
        }
    }

    public override void OnRotationAplication(float movement, HashSet<Cog> visited = null) {
        RotateAroundGears(movement, visited);
    }


    public override void Update(GameTime gameTime) {
        base.Update(gameTime);
    }
}
