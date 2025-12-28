using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineGameB.World.Tiles;

public class Tile(Texture2D texture, Rectangle sourceRectangle, string name) {
    public Texture2D Texture { get; protected set; } = texture;
    public Rectangle SourceRectangle { get; protected set; } = sourceRectangle;
    public int MapLayer = 0;
    public Tile SetMapLayer(int layer) {
        MapLayer = layer;
        return this;
    }

    public bool IsSolid { get; protected set; } = false;
    public Tile SetSolid(bool v = true) {
        IsSolid = v;
        return this;
    }

    public string Name { get; protected set; } = name;
    public Tile SetName(string name) {
        Name = name;
        return this;
    }

    public Color MapColor { get; protected set; } = Color.White;
    public Tile SetMapColor(Color color) {
        MapColor = color;
        return this;
    }

    public Color DrawColor { get; protected set; } = Color.White;
    public Tile SetDrawColor(Color? color=null) {
        DrawColor = color ?? Color.White;
        return this;
    }

    public bool IsLightPassable = true;
    public Tile SetLightPassable(bool v = true) {
        IsLightPassable = v;
        return this;
    }

    public bool IsBreakable { get; protected set; } = false;
    public float Durity { get; protected set; } = float.NaN;
    public Tile SetDurity(float? v = null) {
        IsBreakable = v is not null;
        if (IsBreakable)
            Durity = (float)v;
        else
            Durity = float.NaN;
        return this;
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle position) {
        if (Texture is null)
            return;
        spriteBatch.Draw(Texture, position, SourceRectangle, DrawColor);
    }

    public static Rectangle GetBounds(Vector2 position, int tileSize) {
        return new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize);
    }
}
