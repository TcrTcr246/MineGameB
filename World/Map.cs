using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGame.World.Objects;
using MineGame.World.Tiles;
using System;
using System.Collections.Generic;

namespace MineGame.World;
public class Map {
    public const int TileSize = 32;
    public int Width = 375;
    public int Height = 375;
    public int WorldWidth = 0;
    public int WorldHeight = 0;

    public bool InWorldZoom => Game1.Instance.Camera.Zoom >= 0.35f;
    public Rectangle Rect => new(0, 0, WorldWidth, WorldHeight);

    protected int[,] Tiles;
    protected List<WorldObject> Objects;
    public void AddObject(WorldObject obj) => Objects.Add(obj);

    protected Generator generator;

    public Map() {
        Tiles = new int[Width, Height];
        Objects = [];
        generator = new Generator(Width, Height, TileSize);
    }

    public Map Load() {
        Generate();

        WorldWidth = Width * TileSize;
        WorldHeight = Height * TileSize;
        return this;
    }

    public void Generate() {
        generator.Generate();
        Tiles = generator.Tiles;
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
                        if (Game1.Instance.tileRegister.GetTileById(Tiles[nx, ny]).IsLightPassable) {
                            isLight = true;
                            break;
                        }
                    }

                data[x + y * w] = isLight ? Color.White : Color.Black;
            }

        LightTexture.SetData(data);
    }


    Texture2D drawedTexture;
    KeyboardState ks, lks;
    bool firstFrame = true;

    public void Update(GameTime gameTime) {
        (gameTime).ToString();

        lks = ks;
        ks = Keyboard.GetState();

        if (ks.IsKeyDown(Keys.R) && !lks.IsKeyDown(Keys.R) || firstFrame) {
            Generate();
            GenTex();
            firstFrame = false;
        }

        for (int i = Objects.Count - 1; i >= 0; i--)
            Objects[i].Update(gameTime);
    }

    public void GenTex() {
        const int declatity = 1;

        int w = Width / declatity;
        int h = Height / declatity;
        var flat = new Color[w * h];

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                flat[y * w + x] = Game1.Instance.tileRegister.GetTileById(Tiles[x * declatity, y * declatity]).MapColor;

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
    public Vector2 GetPosAtTile(Point p) {
        return new Vector2(p.X * TileSize, p.Y * TileSize);
    }
    public int GetTileAtWorldPos(Vector2 worldPos) {
        int x = (int)(worldPos.X / TileSize);
        int y = (int)(worldPos.Y / TileSize);
        if (x < 0 || x >= Width || y < 0 || y >= Height) {
            return Game1.Instance.tileRegister.GetIdByName("debug");
        }
        return Tiles[x, y];
    }

    public void Draw(SpriteBatch spriteBatch) {
        if (!InWorldZoom) {
            spriteBatch.Draw(drawedTexture, new Rectangle(0, 0, WorldWidth, WorldHeight), Color.White);
        } else {
            Rectangle r = GetVisibleTileRect(Game1.Instance.Camera.GetViewBounds(Game1.Instance.GraphicsDevice));

            for (int x = r.X; x < r.X + r.Width; x++) {
                for (int y = r.Y; y < r.Y + r.Height; y++) {
                    Tile tile = Game1.Instance.tileRegister.GetTileById(Tiles[x, y]);
                    tile.Draw(spriteBatch, new Rectangle(x*TileSize, y*TileSize, TileSize, TileSize));
                }
            }

            foreach (var o in Objects)
                o.Draw(spriteBatch);

            foreach (var o in Objects)
                if (o is Lever l)
                    l.DrawArm(spriteBatch);
        }
    }

    public void DrawMap(SpriteBatch spriteBatch, Rectangle position) {
        (spriteBatch, position).ToString();
        if (drawedTexture != null) {
            spriteBatch.Draw(drawedTexture, position, Color.White);
        }
    }
}
