using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGameB.Scenes;
using MineGameB.World.Objects;
using MineGameB.World.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MineGameB.World;
public class Map {
    public int TileSize { get; private set; } = 32;
    public int Width { get; private set; } = 1000;
    public int Height { get; private set; } = 1000;
    public int Depth { get; private set; } = 10; // Number of layers
    public int WorldWidth { get; private set; } = 0;
    public int WorldHeight { get; private set; } = 0;

#pragma warning disable IDE0079
#pragma warning disable CA1822
    public bool InWorldZoom => Game1.Instance.Camera.Zoom >= 0.25f;
    bool wasInWorldZoom = false;
#pragma warning restore CA1822
#pragma warning restore IDE0079
    public Rectangle Rect => new(0, 0, WorldWidth, WorldHeight);

    protected int[,,] Tiles; // Now 3D: [x, y, layer]
    protected Dictionary<Point, List<WorldObject>> Objects;

    public WorldObject AddObject(Point pct, WorldObject obj) {
        pct += AddObjectTranslation;
        if (!Objects.TryGetValue(pct, out var list)) {
            list = [];
            Objects[pct] = list;
        }
        list.Add(obj);
        return obj.SetMap(this).SetMapPosition(pct).SetPosition(GetPosAtIndex(pct) + new Vector2(TileSize / 2, TileSize / 2));
    }

    Point AddObjectTranslation = new(0, 0);
    public void TranslateAddObject(Point point) =>
        AddObjectTranslation += point;
    public void ResetTranslationOfAddObject() =>
        AddObjectTranslation = new Point(0, 0);

    public WorldObject AddObjectRel(Point pct, WorldObject obj) => AddObject(pct + new Point(Width / 2, Height / 2), obj);

    protected Generator generator;

    public Map() {
        Tiles = new int[Width, Height, Depth];
        Objects = [];
        generator = new Generator(Width, Height, TileSize, Depth);
    }

    public Map Load() {
        generator.FlatGenerate(GameScene.TileRegister.GetIdByName("floor1"), 0);
        Tiles = generator.Tiles;

        WorldWidth = Width * TileSize;
        WorldHeight = Height * TileSize;
        return this;
    }

    public Map NewGenerate(Func<int, int, int> f, int layer = 0) {
        generator.FuncGenerate(f, layer);
        Tiles = generator.Tiles;
        return this;
    }

    public Map Generate() {
        generator.Generate();
        Tiles = generator.Tiles;
        return this;
    }

    const int darkSeeRange = 2;

    public Texture2D LightTexture;
    public void BuildVisibleLightTexture(Rectangle r) {
        int w = r.Width;
        int h = r.Height;

        if (LightTexture == null ||
            LightTexture.Width != w ||
            LightTexture.Height != h) {
            LightTexture = new Texture2D(
                Game1.Instance.GraphicsDevice, w, h);
        }

        Color[] data = new Color[w * h];

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++) {
                int tx = r.X + x;
                int ty = r.Y + y;

                bool isLight = false;
                for (int _y = -darkSeeRange; _y <= darkSeeRange; _y++)
                    for (int _x = -darkSeeRange; _x <= darkSeeRange; _x++) {
                        int nx = tx + _x;
                        int ny = ty + _y;
                        if (nx < 0 || nx >= Width || ny < 0 || ny >= Height)
                            continue;
                        int topTile = GetTileAtIndex(new Point(nx, ny));
                        if (GameScene.TileRegister.GetTileById(topTile).IsLightPassable) {
                            isLight = true;
                            break;
                        }
                    }

                data[x + y * w] = isLight ? Color.White : Color.Black;
            }

        LightTexture.SetData(data);
    }

    List<Point> modifiedTexPoints = [];
    List<Color> modifiedTexColors = [];

    public void ModifyTex(Point p, Color color) {
        modifiedTexPoints.Add(p);
        modifiedTexColors.Add(color);
    }

    public void ApplyModifTex(Point p, Color color) {
        if (drawedTexture == null)
            return;
        int w = drawedTexture.Width;
        int h = drawedTexture.Height;
        int declatity = Width / w;
        int tx = p.X / declatity;
        int ty = p.Y / declatity;
        if (tx < 0 || tx >= w || ty < 0 || ty >= h)
            return;
        Color[] data = new Color[1];
        drawedTexture.GetData(0, new Rectangle(tx, ty, 1, 1), data, 0, 1);
        data[0] = color;
        drawedTexture.SetData(0, new Rectangle(tx, ty, 1, 1), data, 0, 1);
        modifiedTexPoints.Clear();
        modifiedTexColors.Clear();
    }

    public void GenTex() {
        const int declatity = 1;

        int w = Width / declatity;
        int h = Height / declatity;
        var flat = new Color[w * h];

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++) {
                // Get the top tile at this position
                int topTile = GetTileAtIndex(new Point(x * declatity, y * declatity));
                flat[y * w + x] = GameScene.TileRegister.GetTileById(topTile).MapColor;
            }

        drawedTexture ??= new Texture2D(Game1.Instance.GraphicsDevice, Width / declatity, Height / declatity);
        drawedTexture.SetData(flat);
    }

    public Rectangle GetVisibleTileRect(Rectangle cameraRect) {
        int startX = Math.Max(0, cameraRect.Left / TileSize);
        int startY = Math.Max(0, cameraRect.Top / TileSize);
        int endX = Math.Min(Width - 1, cameraRect.Right / TileSize);
        int endY = Math.Min(Height - 1, cameraRect.Bottom / TileSize);

        return new Rectangle(
            startX - 1,
            startY - 1,
            endX - startX + 3,
            endY - startY + 3
        );
    }

    public Vector2 GetPosAtIndex(Point p, out bool exist) {
        exist = p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;
        return new Vector2(p.X * TileSize, p.Y * TileSize);
    }
    public Vector2 GetPosAtIndex(Point p) => GetPosAtIndex(p, out _);

    public Point GetIndexAtPos(Vector2 worldPos, out bool exist) {
        int x = (int)(worldPos.X / TileSize);
        int y = (int)(worldPos.Y / TileSize);
        exist = x >= 0 && x < Width && y >= 0 && y < Height;
        return new Point(x, y);
    }
    public Point GetIndexAtPos(Vector2 worldPos) => GetIndexAtPos(worldPos, out _);

    public bool IsValidIndex(Point p) =>
        p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;

    public bool IsValidIndex(Point p, int layer) =>
        p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height && layer >= 0 && layer < Depth;

    // Get tile at specific layer, or top tile if layer is null
    public int GetTileAtIndex(Point p, int? layer = null) {
        if (!IsValidIndex(p))
            return GameScene.TileRegister.GetIdByName("debug");

        if (layer.HasValue) {
            // Get specific layer
            if (layer.Value < 0 || layer.Value >= Depth)
                return GameScene.TileRegister.GetIdByName("debug");
            return Tiles[p.X, p.Y, layer.Value];
        } else {
            // Get top non-empty tile
            for (int z = Depth - 1; z >= 0; z--) {
                int tileId = Tiles[p.X, p.Y, z];
                if (tileId != 0) // Assuming 0 is empty/air
                    return tileId;
            }
            return Tiles[p.X, p.Y, 0]; // Return bottom layer if all empty
        }
    }

    // Set tile at specific layer, or add on top if layer is null
    public void SetTileAtIndex(Point p, int id, int? layer = null) {
        if (!IsValidIndex(p))
            return;

        int targetLayer;

        if (layer.HasValue) {
            // Set at specific layer
            if (layer.Value < 0 || layer.Value >= Depth)
                return;
            targetLayer = layer.Value;
        } else {
            // Find first empty layer from top
            targetLayer = -1;
            for (int z = 0; z < Depth; z++) {
                if (Tiles[p.X, p.Y, z] == 0) {
                    targetLayer = z;
                    break;
                }
            }

            // If no empty layer found, use top layer
            if (targetLayer == -1)
                targetLayer = Depth - 1;
        }

        Tiles[p.X, p.Y, targetLayer] = id;

        // Update minimap texture to show top tile
        int topTile = GetTileAtIndex(p);
        ModifyTex(p, GameScene.TileRegister.GetTileById(topTile).GetMapColor());
    }

    // Remove the top tile at position
    public void RemoveTileAtIndex(Point p) {
        if (!IsValidIndex(p))
            return;

        // Find and remove the topmost non-empty tile
        for (int z = Depth - 1; z >= 0; z--) {
            if (Tiles[p.X, p.Y, z] != 0) {
                Tiles[p.X, p.Y, z] = 0;

                // Update minimap texture to show new top tile
                int topTile = GetTileAtIndex(p);
                ModifyTex(p, GameScene.TileRegister.GetTileById(topTile).GetMapColor());
                return;
            }
        }
    }

    public Tile GetTileObjectAtIndex(Point p, int? layer = null) =>
        GameScene.TileRegister.GetTileById(GetTileAtIndex(p, layer));

    public string GetTileNameAtIndex(Point p, int? layer = null) =>
        GameScene.TileRegister.GetTileById(GetTileAtIndex(p, layer)).Name;

    public int GetTileAtWorldPos(Vector2 worldPos, int? layer = null) {
        var p = GetIndexAtPos(worldPos, out var exist);
        return exist ? GetTileAtIndex(p, layer) : GameScene.TileRegister.GetIdByName("debug");
    }

    public List<WorldObject> GetObjectListAtPos(Point loc) =>
        Objects.GetValueOrDefault(loc);

    public static WorldObject FindObject(List<WorldObject> items, Func<WorldObject, bool> predicate) =>
        items?.FirstOrDefault(predicate);


    Texture2D drawedTexture;
    KeyboardState ks, lks;
    bool firstFrame = true;
    bool JustOutsideWorldZoom = false;

    public void Update(GameTime gameTime) {
        lks = ks;
        ks = Keyboard.GetState();

        if (ks.IsKeyDown(Keys.R) && !lks.IsKeyDown(Keys.R)) {
            Generate();
            firstFrame = true;
        }

        JustOutsideWorldZoom = !InWorldZoom && !wasInWorldZoom;
        wasInWorldZoom = !InWorldZoom;
        if (firstFrame || JustOutsideWorldZoom) {
            GenTex();
        }
        firstFrame = false;

        foreach (var o in Objects.Values.Reverse())
            foreach (var o2 in o)
                o2.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch) {
        if (!InWorldZoom) {
            spriteBatch.Draw(drawedTexture, new Rectangle(0, 0, WorldWidth, WorldHeight), Color.White);
        } else {
            Rectangle r = GetVisibleTileRect(Game1.Instance.Camera.GetViewBounds(Game1.Instance.GraphicsDevice));

            // Draw only the top tile at each position
            for (int x = r.X; x < r.X + r.Width; x++) {
                for (int y = r.Y; y < r.Y + r.Height; y++) {
                    if (!IsValidIndex(new Point(x, y)))
                        continue;

                    int topTileId = GetTileAtIndex(new Point(x, y));
                    Tile tile = GameScene.TileRegister.GetTileById(topTileId);
                    tile.Draw(spriteBatch, new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize));
                }
            }

            int layerRange = 10;

            // Draw objects with their layer system
            foreach (var list in Objects.Values)
                for (int n = -layerRange; n < 0; n++)
                    foreach (var o in list)
                        DrawIfVisible(o, () => o.DrawLayer(spriteBatch, n));

            foreach (var list in Objects.Values)
                foreach (var o in list)
                    DrawIfVisible(o, () => o.Draw(spriteBatch));

            foreach (var list in Objects.Values)
                for (int n = 0; n <= layerRange; n++)
                    foreach (var o in list)
                        DrawIfVisible(o, () => o.DrawLayer(spriteBatch, n));
        }
    }

    static void DrawIfVisible(WorldObject o, Action draw) {
        if (Game1.Instance.Camera.GetViewBounds(Game1.Instance.GraphicsDevice).Intersects(o.GetBounds()))
            draw();
    }

    public void DrawMap(SpriteBatch spriteBatch, Rectangle position) {
        (spriteBatch, position).ToString();
        if (drawedTexture != null) {
            spriteBatch.Draw(drawedTexture, position, Color.White);
        }
    }
}