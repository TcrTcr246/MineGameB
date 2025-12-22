using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineGameB.World;

namespace MineGameB.Effects;

public class ShadowEffect : Effect {
    private readonly Map map;
    private readonly Texture2D texture;
    private Rectangle mapLightningRect;

    public ShadowEffect(Microsoft.Xna.Framework.Graphics.Effect effect, Map map, Texture2D shadowTex) : base(effect) {
        this.map = map;
        texture = shadowTex;
    }

    public override void Update(GameTime gameTime) {
        if (!map.InWorldZoom)
            return;

        var cam = Game1.Instance.Camera;

        Rectangle camRect = cam.GetViewBounds(Game1.Instance.GraphicsDevice);
        Rectangle r = map.GetVisibleTileRect(camRect);

        var mapLightningOffset = map.GetPosAtIndex(new Point(r.X, r.Y));
        mapLightningRect = new Rectangle(
            (int)mapLightningOffset.X,
            (int)mapLightningOffset.Y,
            r.Width * map.TileSize,
            r.Height * map.TileSize
        );

        map.BuildVisibleLightTexture(r);

        effect.Parameters["TileSize"]
            .SetValue(1f / (map.TileSize / cam.Zoom));
        effect.Parameters["Softness"]
            .SetValue(2f);
    }

    public override void Draw(SpriteBatch spriteBatch) {
        if (!map.InWorldZoom)
            return;

        Game1.Instance.GraphicsDevice.Textures[0] = texture;

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: Game1.Instance.CameraTransform, effect: this.effect);
        spriteBatch.Draw(map.LightTexture, mapLightningRect, Color.White);
        spriteBatch.End();
    }
}
