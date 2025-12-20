using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineGameB.World.Objects;
public class WorldObject {
    public Point TilePosition { get; protected set; }
    public Vector2 Position { get; protected set; }
    protected Map MapRef { get; private set; }

    public WorldObject SetMap(Map map) {
        MapRef = map;
        return this;
    }

    public WorldObject SetPosition(Vector2 pos) {
        Position = pos;
        return this;
    }
    public virtual void OnSetMapPosition(Point pos) { }
    public WorldObject SetMapPosition(Point pos) {
        TilePosition = pos;
        OnSetMapPosition(pos);
        return this;
    }

    public virtual WorldObject Load() {
        return this;
    }

    public virtual void Update(GameTime gameTime) { }


    public virtual void DrawLayer(SpriteBatch spriteBatch, int layer) { }
    public virtual void Draw(SpriteBatch spriteBatch) { }
}
