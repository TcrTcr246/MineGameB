using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGameB.Scenes;
using MineGameB.World.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MineGameB.World;
public class Map {
    public int TileSize { get; private set; } = 32;
    public int Width { get; private set; } = 375;
    public int Height { get; private set; } = 375;
    public int WorldWidth { get; private set; } = 0;
    public int WorldHeight { get; private set; } = 0;

#pragma warning disable IDE0079
#pragma warning disable CA1822
    public bool InWorldZoom => Game1.Instance.Camera.Zoom >= 0.2f; // 0.35f
#pragma warning restore CA1822
#pragma warning restore IDE0079
    public Rectangle Rect => new(0, 0, WorldWidth, WorldHeight);

    protected int[,] Tiles;
    protected Dictionary<Point, List<WorldObject>> Objects;
    public WorldObject AddObject(Point pct, WorldObject obj) {
        pct += AddObjectTranslation;
        if (!Objects.TryGetValue(pct, out var list)) {
            list = [];
            Objects[pct] = list;
        }
        list.Add(obj);
        return obj.SetMap(this).SetMapPosition(pct).SetPosition(GetPosAtIndex(pct)+new Vector2(TileSize / 2, TileSize / 2));
    }

    Point AddObjectTranslation = new(0, 0);
    public void TranslateAddObject(Point point) =>
        AddObjectTranslation += point;
    public void ResetTranslationOfAddObject() =>
        AddObjectTranslation = new Point(0, 0);

    public WorldObject AddObjectRel(Point pct, WorldObject obj) => AddObject(pct + new Point(Width/2, Height/2), obj);

    protected Generator generator;

    public Map() {
        Tiles = new int[Width, Height];
        Objects = [];
        generator = new Generator(Width, Height, TileSize);
    }

    public Map Load() {
        generator.FlatGenerate(GameScene.TileRegister.GetIdByName("floor1"));
        Tiles = generator.Tiles;

        WorldWidth = Width * TileSize;
        WorldHeight = Height * TileSize;
        return this;
    }
    public Map NewGenerate(Func<int, int, int> f) {
        generator.FuncGenerate(f);
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
                for (int _y=-darkSeeRange; _y<= darkSeeRange; _y++)
                    for (int _x=-darkSeeRange; _x<= darkSeeRange; _x++) {
                        int nx = tx + _x;
                        int ny = ty + _y;
                        if (nx < 0 || nx >= Width || ny < 0 || ny >= Height) continue;
                        if (GameScene.TileRegister.GetTileById(Tiles[nx, ny]).IsLightPassable) {
                            isLight = true;
                            break;
                        }
                    }

                data[x + y * w] = isLight ? Color.White : Color.Black;
            }

        LightTexture.SetData(data);
    }

    public void GenTex() {
        const int declatity = 1;

        int w = Width / declatity;
        int h = Height / declatity;
        var flat = new Color[w * h];

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                flat[y * w + x] = GameScene.TileRegister.GetTileById(Tiles[x * declatity, y * declatity]).MapColor;

        drawedTexture ??= new Texture2D(Game1.Instance.GraphicsDevice, Width/declatity, Height/declatity);
        drawedTexture.SetData(flat);
    }

    public Rectangle GetVisibleTileRect(Rectangle cameraRect) {
        int startX = Math.Max(0, cameraRect.Left / TileSize);
        int endX = Math.Min(Width - 1, cameraRect.Right / TileSize);
        int startY = Math.Max(0, cameraRect.Top / TileSize);
        int endY = Math.Min(Height - 1, cameraRect.Bottom / TileSize);

        return new Rectangle(
            startX,
            startY,
            endX - startX + 1,
            endY - startY + 1
        );
    }
    public Vector2 GetPosAtIndex(Point p, out bool exist) {
        int x = p.X, y = p.Y;
        exist = true;
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            exist = false;
        return new Vector2(p.X * TileSize, p.Y * TileSize);
    }
    public Vector2 GetPosAtIndex(Point p) => GetPosAtIndex(p, out var _);


    public Point GetIndexAtPos(Vector2 worldPos, out bool exist) {
        int x = (int)(worldPos.X / TileSize);
        exist = true;
        int y = (int)(worldPos.Y / TileSize);
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            exist = false;
        return new(x, y);
    }
    public Point GetIndexAtPos(Vector2 worldPos) => GetIndexAtPos(worldPos, out var _);

    public int GetTileAtWorldPos(Vector2 worldPos) {
        var p = GetIndexAtPos(worldPos, out var exist);
        if (!exist)
            return GameScene.TileRegister.GetIdByName("debug");
        return Tiles[p.X, p.Y];
    }

    public List<WorldObject> GetObjectListAtPos(Point loc) => Objects.GetValueOrDefault(loc);

    public static WorldObject FindObject(List<WorldObject> items, Func<WorldObject, bool> f) {
        foreach (WorldObject obj in items)
            if (f(obj))
                return obj;
        return null;
    }


    Texture2D drawedTexture;
    KeyboardState ks, lks;
    bool firstFrame = true;

    public void Update(GameTime gameTime) {
        lks = ks;
        ks = Keyboard.GetState();

        if (ks.IsKeyDown(Keys.R) && !lks.IsKeyDown(Keys.R)) {
            Generate();
            GenTex();
        }
        if (firstFrame)
            GenTex();
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

            for (int x = r.X; x < r.X + r.Width; x++) {
                for (int y = r.Y; y < r.Y + r.Height; y++) {
                    Tile tile = GameScene.TileRegister.GetTileById(Tiles[x, y]);
                    tile.Draw(spriteBatch, new Rectangle(x*TileSize, y*TileSize, TileSize, TileSize));
                }
            }

            int layerRange = 10;

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
