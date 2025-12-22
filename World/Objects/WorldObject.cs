using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineGameB.World.Objects;
public class WorldObject {
    public Point TilePosition { get; protected set; }
    public Vector2 Position { get; protected set; }
    protected Map MapRef { get; private set; }
    public int MapLayer { get; protected set; } = 5;
    public WorldObject SetMapLayer(int n) {
        MapLayer = n;
        return this;
    }

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

    public virtual Rectangle GetBounds() {
        return new((int)Position.X, (int)Position.Y, 0, 0);
    }
}
