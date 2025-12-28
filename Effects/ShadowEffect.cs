using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineGameB.World;

namespace MineGameB.Effects;

public class ShadowEffect : Effect {
    private readonly Map map;
    private readonly Texture2D texture;
    private Rectangle mapLightningRect;
    private Rectangle lastVisibleRect;
    private int updateCounter = 0;
    private const int UPDATE_INTERVAL = 2; // Update every 2 frames
    private const int LIGHT_SCALE = 2; // Build light map at 1/2 resolution

    public ShadowEffect(Microsoft.Xna.Framework.Graphics.Effect effect, Map map, Texture2D shadowTex) : base(effect) {
        this.map = map;
        texture = shadowTex;
        lastVisibleRect = Rectangle.Empty;
    }

    public override void Update(GameTime gameTime) {
        if (!map.InWorldZoom)
            return;

        var cam = Game1.Instance.Camera;
        Rectangle camRect = cam.GetViewBounds(Game1.Instance.GraphicsDevice);
        Rectangle r = map.GetVisibleTileRect(camRect);

        // Only update if view changed significantly or every N frames
        updateCounter++;
        if (r != lastVisibleRect || updateCounter >= UPDATE_INTERVAL) {
            var mapLightningOffset = map.GetPosAtIndex(new Point(r.X, r.Y));
            mapLightningRect = new Rectangle(
                (int)mapLightningOffset.X,
                (int)mapLightningOffset.Y,
                r.Width * map.TileSize,
                r.Height * map.TileSize
            );

            map.BuildVisibleLightTexture(r);
            lastVisibleRect = r;
            updateCounter = 0;
        }

        effect.Parameters["TileSize"]
            .SetValue(LIGHT_SCALE / (map.TileSize / cam.Zoom));
        effect.Parameters["Softness"]
            .SetValue(2f);
    }

    public override void Draw(SpriteBatch spriteBatch) {
        if (!map.InWorldZoom)
            return;

        Game1.Instance.GraphicsDevice.Textures[0] = texture;

        // Use bilinear filtering to smooth the upscaled texture
        spriteBatch.Begin(
            SpriteSortMode.Immediate,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp, // Smooth upscaling
            null,
            null,
            this.effect,
            Game1.Instance.CameraTransform
        );

        spriteBatch.Draw(map.LightTexture, mapLightningRect, Color.White);
        spriteBatch.End();
    }
}