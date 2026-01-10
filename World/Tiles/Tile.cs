using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineGameB.Scenes;
using System;

namespace MineGameB.World.Tiles;

public class Tile {
    public Tile(Texture2D texture, Rectangle sourceRectangle, string name) {
        Texture = texture;
        SourceRectangle = sourceRectangle;
        Name = name;
    }

    public int Id { get; private set; }
    internal Tile SetId(int id) {
        Id = id;
        return this;
    }

    public Texture2D Texture { get; private set; }
    public Rectangle SourceRectangle { get; private set; }

    public bool IsSolid { get; private set; } = false;
    public Tile SetSolid(bool v = true) {
        IsSolid = v;
        return this;
    }

    public string Name { get; private set; } = "unnamed";
    public Tile SetName(string name) {
        Name = name;
        return this;
    }

    public Color MapColor { get; private set; } = Color.White;
    public Tile SetMapColor(Color color) {
        MapColor = color;
        return this;
    }

    public Color DrawColor { get; private set; } = Color.White;
    public Tile SetDrawColor(Color? color = null) {
        DrawColor = color ?? Color.White;
        return this;
    }

    public bool IsLightPassable { get; private set; } = true;
    public Tile SetLightPassable(bool v = true) {
        IsLightPassable = v;
        return this;
    }

    public Func<int, int, int> TransformIntoAfterCover { get; private set; } = (myId, overId) => myId;
    public Tile SetTransformIntoAfterCovert(Func<int, int, int> func) {
        TransformIntoAfterCover = func;
        return this;
    }

    internal int OnCover(Tile tile) => TransformIntoAfterCover(Id, tile.Id);

    public Func<int, int> DropFunc { get; private set; } = myId => myId;
    public Tile SetOnBreak(Func<int, int> func) {
        DropFunc = func;
        return this;
    }

    internal void OnBreak() {
        var id = DropFunc(Id);
        var tile = GameScene.TileRegister.GetTileById(id);
        GameScene.Inventory.AddItem(tile.Texture, tile.SourceRectangle, tile.Name, 1);
    }

    public bool IsBreakable { get; private set; } = false;
    public float Durity { get; private set; } = float.NaN;
    public Tile SetDurity(float? v = null) {
        IsBreakable = v is not null;
        if (IsBreakable)
            Durity = (float)v;
        else
            Durity = float.NaN;
        return this;
    }

    internal void Draw(SpriteBatch spriteBatch, Rectangle position) {
        if (Texture is null)
            return;
        var pos = new Rectangle(
            (int)Math.Round((float)position.X),
            (int)Math.Round((float)position.Y),
            (int)Math.Round((float)position.Width),
            (int)Math.Round((float)position.Height)
        );
        spriteBatch.Draw(Texture, pos, SourceRectangle, DrawColor);
    }

    internal static Rectangle GetBounds(Vector2 position, int tileSize) {
        return new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize);
    }
}