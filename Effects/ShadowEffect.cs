using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineGameB.World;

namespace MineGameB.Effects;
public class ShadowEffect : Effect {
    Map map;
    Rectangle mapLightningRect;
    protected Texture2D texture;

    public ShadowEffect(Microsoft.Xna.Framework.Graphics.Effect effect, Map map, Texture2D shadowTex) : base(effect) {
        this.map = map;
        this.texture = shadowTex;
    }

    public override void Draw(SpriteBatch spriteBatch) {
        if (!map.InWorldZoom)
            return;
        Rectangle camRect = Game1.Instance.Camera.GetViewBounds(Game1.Instance.GraphicsDevice);
        Rectangle r = map.GetVisibleTileRect(camRect);
        int border = 1;
        r = new Rectangle(r.X - border, r.Y - border, r.Width + border * 2, r.Height + border * 2);

        var mapLightningOffset = map.GetPosAtIndex(new Point(r.X, r.Y));
        mapLightningRect = new Rectangle(
            (int)mapLightningOffset.X,
            (int)mapLightningOffset.Y,
            r.Width * map.TileSize,
            r.Height * map.TileSize
        );

        map.BuildVisibleLightTexture(r);

        // bind texture to shader slot s0
        Game1.Instance.GraphicsDevice.Textures[0] = texture;

        // shader params
        effect.Parameters["TileSize"].SetValue(1f / (map.TileSize / Game1.Instance.Camera.Zoom));
        effect.Parameters["Softness"].SetValue(2f);

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: Game1.Instance.CameraTransform, effect: this.effect);
        if (map.InWorldZoom) {
            spriteBatch.Draw(map.LightTexture, mapLightningRect, Color.White);
        }
        spriteBatch.End();
    }
}
