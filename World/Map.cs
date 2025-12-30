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
    public int Depth { get; private set; } = 4;
    public int WorldWidth { get; private set; } = 0;
    public int WorldHeight { get; private set; } = 0;

#pragma warning disable IDE0079
#pragma warning disable CA1822
    public bool InWorldZoom => Game1.Instance.Camera.Zoom >= 0.25f;
    bool wasInWorldZoom = false;
#pragma warning restore CA1822
#pragma warning restore IDE0079
    public Rectangle Rect => new(0, 0, WorldWidth, WorldHeight);

    protected int[,,] Tiles;
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

    public Generator Generator;

    public int GetSeed() => (int)Generator.Seed;

    public Map() {
        Tiles = new int[Width, Height, Depth];
        Objects = [];
        Generator = new Generator(Width, Height, TileSize, Depth);
        Breaks = [];

        Load();
    }

    private Texture2D breakOverlay;
    public Map Load() {
        Generator.RandomSeed();
        Generator.FlatGenerate(GameScene.TileRegister.GetIdByName("floor1"), 0);
        Tiles = Generator.Tiles;
        breakOverlay = Game1.Instance.Content.Load<Texture2D>("TileCrackOverlay");

        WorldWidth = Width * TileSize;
        WorldHeight = Height * TileSize;
        return this;
    }

    public Map NewFastGenerate(Func<int, int, int> f, int layer = 0) {
        Generator.FuncGenerate(f, layer);
        Tiles = Generator.Tiles;
        return this;
    }

    public Map NewGenerate(int[,] tileIds) {
        if (tileIds == null)
            throw new ArgumentNullException(nameof(tileIds));

        int tilesWidth = tileIds.GetLength(0);
        int tilesHeight = tileIds.GetLength(1);

        if (tilesWidth != Width || tilesHeight != Height)
            throw new ArgumentException($"Tile array dimensions ({tilesWidth}x{tilesHeight}) don't match map dimensions ({Width}x{Height})");

        // Clear all tiles first
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                for (int z = 0; z < Depth; z++) {
                    Tiles[x, y, z] = 0;
                }
            }
        }

        // Copy the provided tile IDs to the base layer
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                Tiles[x, y, 0] = tileIds[x, y];
            }
        }

        return this;
    }

    public Map NewGenerate(int[,,] tileIds) {
        if (tileIds == null)
            throw new ArgumentNullException(nameof(tileIds));

        int tilesWidth = tileIds.GetLength(0);
        int tilesHeight = tileIds.GetLength(1);
        int tilesDepth = tileIds.GetLength(2);

        if (tilesWidth != Width || tilesHeight != Height || tilesDepth != Depth)
            throw new ArgumentException($"Tile array dimensions ({tilesWidth}x{tilesHeight}x{tilesDepth}) don't match map dimensions ({Width}x{Height}x{Depth})");

        // Copy the provided tile IDs to all layers
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                for (int z = 0; z < Depth; z++) {
                    Tiles[x, y, z] = tileIds[x, y, z];
                }
            }
        }

        return this;
    }

    const int darkSeeRange = 1;

    public Texture2D LightTexture;
    // Optimized BuildVisibleLightTexture for your Map class

    private Color[] lightDataCache; // Reuse array to avoid allocations

    public void BuildVisibleLightTexture(Rectangle r) {
        int w = r.Width;
        int h = r.Height;
        if (!(w > 0 && h > 0))
            return;

        // Reuse texture if possible
        if (LightTexture == null ||
            LightTexture.Width != w ||
            LightTexture.Height != h) {
            LightTexture?.Dispose();
            LightTexture = new Texture2D(Game1.Instance.GraphicsDevice, w, h);
            lightDataCache = new Color[w * h];
        }

        // Reuse array instead of allocating new one each frame
        Color[] data = lightDataCache;

        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                int tx = r.X + x;
                int ty = r.Y + y;
                bool isLight = false;

                // Check surrounding tiles for light passability
                for (int _y = -darkSeeRange; _y <= darkSeeRange; _y++) {
                    for (int _x = -darkSeeRange; _x <= darkSeeRange; _x++) {
                        int nx = tx + _x;
                        int ny = ty + _y;

                        // Bounds check
                        if (nx < 0 || nx >= Width || ny < 0 || ny >= Height)
                            continue;

                        int topTile = GetTileAtIndex(new Point(nx, ny));
                        if (GameScene.TileRegister.GetTileById(topTile).IsLightPassable) {
                            isLight = true;
                            goto FoundLight; // Break out of both loops
                        }
                    }
                }

                FoundLight:
                data[x + y * w] = isLight ? Color.White : Color.Black;
            }
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
    }

    public void ClearModifiedTex() {
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
                int topTile = GetTileAtIndex(new Point(x * declatity, y * declatity));
                flat[y * w + x] = GameScene.TileRegister.GetTileById(topTile).MapColor;
            }

        drawedTexture ??= new Texture2D(Game1.Instance.GraphicsDevice, Width / declatity, Height / declatity);
        drawedTexture.SetData(flat);
    }

    public Rectangle GetVisibleTileRect(Rectangle cameraRect, int border=1) {
        int startX = Math.Max(0, cameraRect.Left / TileSize);
        int startY = Math.Max(0, cameraRect.Top / TileSize);
        int endX = Math.Min(Width - 1, cameraRect.Right / TileSize);
        int endY = Math.Min(Height - 1, cameraRect.Bottom / TileSize);

        return new Rectangle(
            startX - border,
            startY - border,
            endX - startX + 1 + border * 2,
            endY - startY + 1 + border * 2
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
    public int GetTileAtIndex(Point p, int? layer = null) {
        if (!IsValidIndex(p))
            return GameScene.TileRegister.GetIdByName("debug");

        if (layer.HasValue) {
            if (layer.Value < 0 || layer.Value >= Depth)
                return GameScene.TileRegister.GetIdByName("debug");
            return Tiles[p.X, p.Y, layer.Value];
        } else {
            for (int z = Depth - 1; z >= 0; z--) {
                int tileId = Tiles[p.X, p.Y, z];
                if (tileId != 0)
                    return tileId;
            }
            return Tiles[p.X, p.Y, 0];
        }
    }

    public void SetTileAtIndex(Point p, int id, int? layer = null) {
        if (!IsValidIndex(p))
            return;
        int targetLayer;
        if (layer.HasValue) {
            if (layer.Value < 0 || layer.Value >= Depth)
                return;
            targetLayer = layer.Value;
        } else {
            targetLayer = -1;
            for (int z = 0; z < Depth; z++) {
                if (Tiles[p.X, p.Y, z] == 0) {
                    targetLayer = z;
                    break;
                }
            }
            if (targetLayer == -1)
                targetLayer = Depth - 1;
        }

        Tiles[p.X, p.Y, targetLayer] = id;

        // Apply OnCover to the tile that was just placed if there's a tile above it
        if (targetLayer > 0) {
            int tileAboveId = Tiles[p.X, p.Y, targetLayer - 1];
            if (tileAboveId != 0) {
                var tileAbove = GameScene.TileRegister.GetTileById(tileAboveId);
                int transformedId = tileAbove.OnCover(tileAbove);
                Tiles[p.X, p.Y, targetLayer - 1] = transformedId;
            }
        }

        int topTile = GetTileAtIndex(p);
        ModifyTex(p, GameScene.TileRegister.GetTileById(topTile).MapColor);
    }

    public void RemoveTileAtIndex(Point p) {
        if (!IsValidIndex(p))
            return;

        for (int z = Depth - 1; z >= 0; z--) {
            if (Tiles[p.X, p.Y, z] != 0) {
                Tiles[p.X, p.Y, z] = 0;

                int topTile = GetTileAtIndex(p);
                ModifyTex(p, GameScene.TileRegister.GetTileById(topTile).MapColor);
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


    // break
    protected Dictionary<Point, TileBreakData> Breaks;
    private float regenDelay = 1f; // Seconds before regeneration starts
    private float regenSpeed = 60f; // Break points regenerated per second

    public class TileBreakData {
        public float BreakAmount; // Changed from int to float for smooth regeneration
        public float LastHitTime;

        public TileBreakData(float breakAmount, float lastHitTime) {
            BreakAmount = breakAmount;
            LastHitTime = lastHitTime;
        }
    }

    public void BreakTileAtIndex(Point p, GameTime gameTime) {
        int topTileId = GetTileAtIndex(p);
        Tile tile = GameScene.TileRegister.GetTileById(topTileId);

        if (tile?.IsBreakable != true)
            return;

        int maxBreaks = (int)(tile.Durity * 10);
        float currentTime = (float)gameTime.TotalGameTime.TotalSeconds;

        if (!Breaks.TryGetValue(p, out TileBreakData breakData)) {
            breakData = new TileBreakData(0, currentTime);
            Breaks[p] = breakData;
        }

        breakData.BreakAmount += (float)gameTime.ElapsedGameTime.TotalSeconds * 1000;
        breakData.LastHitTime = currentTime;

        if (breakData.BreakAmount >= maxBreaks) {
            tile.OnBreak();
            RemoveTileAtIndex(p);
            Breaks.Remove(p);
        }
    }

    public void UpdateBreak(GameTime gameTime) {
        float currentTime = (float)gameTime.TotalGameTime.TotalSeconds;
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        List<Point> toRemove = new List<Point>();

        foreach (var kvp in Breaks) {
            Point pos = kvp.Key;
            TileBreakData breakData = kvp.Value;

            float timeSinceHit = currentTime - breakData.LastHitTime;

            if (timeSinceHit >= regenDelay) {
                // Regenerate using delta time
                float regenAmount = regenSpeed * deltaTime;
                breakData.BreakAmount = Math.Max(0, breakData.BreakAmount - regenAmount);

                if (breakData.BreakAmount <= 0) {
                    toRemove.Add(pos);
                }
            }
        }

        foreach (Point pos in toRemove) {
            Breaks.Remove(pos);
        }
    }

    private int breakOverlayFrames = 32; // Number of frames in your texture

    public void DrawBreakOverlay(SpriteBatch spriteBatch) {
        foreach (var kvp in Breaks) {
            Point tilePos = kvp.Key;
            TileBreakData breakData = kvp.Value;

            int topTileId = GetTileAtIndex(tilePos);
            Tile tile = GameScene.TileRegister.GetTileById(topTileId);

            if (tile == null)
                continue;

            int maxBreaks = (int)(tile.Durity * 10);
            float breakProgress = breakData.BreakAmount / maxBreaks;

            // Calculate which frame to show
            int frameIndex = Math.Min((int)(breakProgress * breakOverlayFrames), breakOverlayFrames - 1);

            // Calculate source rectangle (assuming frames are horizontal)
            Rectangle sourceRect = new Rectangle(
                frameIndex * TileSize,
                0,
                TileSize,
                TileSize
            );

            // Convert tile position to screen position
            Vector2 screenPos = new Vector2(tilePos.X * TileSize, tilePos.Y * TileSize);

            // Draw the appropriate frame
            spriteBatch.Draw(breakOverlay, screenPos, sourceRect, Color.White);
        }
    }

    // break end


    public bool TouchesAnySolidTile(Rectangle player) {
        int startX = player.Left / TileSize;
        int endX = (player.Right - 1) / TileSize;
        int startY = player.Top / TileSize;
        int endY = (player.Bottom - 1) / TileSize;

        for (int x = startX; x <= endX; x++) {
            for (int y = startY; y <= endY; y++) {
                if (!IsValidIndex(new Point(x, y)))
                    continue;

                int tileId = GetTileAtIndex(new Point(x, y));
                if (tileId == 0)
                    continue;

                Tile tile = GameScene.TileRegister.GetTileById(tileId);
                if (tile.IsSolid)
                    return true;
            }
        }

        return false;
    }


    Texture2D drawedTexture;
    KeyboardState ks, lks;
    bool firstFrame = true;
    bool JustOutsideWorldZoom = false;

    public void Update(GameTime gameTime) {
        lks = ks;
        ks = Keyboard.GetState();

        JustOutsideWorldZoom = !InWorldZoom && !wasInWorldZoom;
        wasInWorldZoom = !InWorldZoom;
        if (firstFrame || JustOutsideWorldZoom) {
            for (int i = 0; i < modifiedTexPoints.Count; i++)
                ApplyModifTex(modifiedTexPoints[i], modifiedTexColors[i]);
            ClearModifiedTex();
        }
        firstFrame = false;

        foreach (var o in Objects.Values.Reverse())
            foreach (var o2 in o)
                o2.Update(gameTime);

        UpdateBreak(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch) {
        if (!InWorldZoom) {
            spriteBatch.Draw(drawedTexture, new Rectangle(0, 0, WorldWidth, WorldHeight), Color.White);
        } else {
            Rectangle r = GetVisibleTileRect(Game1.Instance.Camera.GetViewBounds());

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

            DrawBreakOverlay(spriteBatch);
        }
    }

    static void DrawIfVisible(WorldObject o, Action draw) {
        if (Game1.Instance.Camera.GetViewBounds().Intersects(o.GetBounds()))
            draw();
    }

    public void DrawMap(SpriteBatch spriteBatch, Rectangle position) {
        (spriteBatch, position).ToString();
        spriteBatch.Draw(drawedTexture, position, Color.White);
    }
}