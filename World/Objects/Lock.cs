using GameTemplate;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGameB.Misc;

namespace MineGameB.World.Objects;

public class Lock : WorldObject {
    private Texture2D texture;
    Rectangle sourceDown = new(32, 32, 16, 16);
    Rectangle sourceUp = new(48, 32, 16, 16);
    Vector2 basePivot = new(8f, 8f);
    public Vector2 Scale = new(1f, 1f);
    protected bool pressed = false;
    public Lock(Texture2D texture) : base() {
        this.texture = texture;
        Load();
    }

    Cog cogweel = null;

    public override void DrawLayer(SpriteBatch spriteBatch, int layer) {
        switch (layer) {
            case 2:
            spriteBatch.Draw(
                texture,
                Position,
                pressed ? sourceDown : sourceUp,
                Color.White,
                0f,
                basePivot,
                Scale,
                SpriteEffects.None,
                0f
            );
            break;
        }
    }
    public override void OnSetMapPosition(Point pos) {
        cogweel = Lever.GetCog(MapRef, pos);
        pressed = cogweel.IsActive();
    }

    Circle Hitbox => new(Position, 8f * Scale.X);

    MouseState ms, lms;
    public override void Update(GameTime gameTime) {
        lms = ms;
        ms = Mouse.GetState();

        var mousePos = Game1.Instance.Camera.MouseWorld;

        bool click = ms.LeftButton == ButtonState.Pressed &&
                     lms.LeftButton == ButtonState.Released;

        if (Hitbox.Contains(mousePos) && click) {
            cogweel.ToggleActive();
            pressed = cogweel.IsActive();
        }
    }
}