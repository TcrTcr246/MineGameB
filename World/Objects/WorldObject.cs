using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineGame.World.Objects;
public class WorldObject {
    public Vector2 Position { get; set; }

    public virtual WorldObject Load() {
        return this;
    }

    public virtual void Update(GameTime gameTime) { }
    public virtual void Draw(SpriteBatch spriteBatch) { }
}
