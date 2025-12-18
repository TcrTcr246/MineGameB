using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineGame.World;

public class Tile(Texture2D texture, Rectangle sourceRectangle, string name) {
    public Texture2D Texture { get; set; } = texture;
    public Rectangle SourceRectangle { get; set; } = sourceRectangle;

    public string Name { get; set; } = name;
    public Tile SetName(string name) {
        Name = name;
        return this;
    }

    public bool IsSolid { get; set; } = false;
    public Tile SetSolid (bool v = false) {
        IsSolid = v;
        return this;
    }

    public Color MapColor { get; set; } = Color.White;
    public Tile SetMapColor(Color color) {
        MapColor = color;
        return this;
    }

    public bool IsLightPassable = true;
    public Tile SetLightPassable(bool v = true) {
        IsLightPassable = v;
        return this;
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle position) {
        spriteBatch.Draw(Texture, position, SourceRectangle, Color.White);
    }

    public static Rectangle GetBounds(Vector2 position, int tileSize) {
        return new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize);
    }
}
