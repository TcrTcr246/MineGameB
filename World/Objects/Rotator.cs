using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace MineGameB.World.Objects;
public class Rotator : Lever {
    public Rotator(Texture2D texture, Cog cogweel = null) : base(texture, cogweel) {
        baseASource = new Rectangle(64, 0, 32, 32);
    }
    public override void DrawLayer(SpriteBatch spriteBatch, int layer) {
        if (layer == -2) {
            if (!cogAttached)
                DrawBaseA(spriteBatch);
        }
    }

    public override void Update(GameTime gameTime) { }
}
