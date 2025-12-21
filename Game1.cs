using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGameB.World;
using MineGameB.World.Objects;
using MineGameB.World.Tiles;

namespace MineGameB;
public class Game1 : GameTemplate.Game {
    public static Game1 Instance { get; private set; }

    protected override void Initialize() {
        Shapes.Initialize(GraphicsDevice);
        Camera.TranslateBackToWorldPos = false;
        base.Initialize();
    }

    protected override void Dispose(bool disposing) {
        if (disposing)
            Shapes.Dispose();
        base.Dispose(disposing);
    }


    Map map;
    Texture2D dotTex, shadowTex;
    Effect radialEffect;
    public TileRegister tileRegister;

    public Game1() : base() {
        Instance = this;
    }

    protected override void LoadContent() {
        dotTex = new Texture2D(GraphicsDevice, 1, 1);
        dotTex.SetData([Color.White]);

        radialEffect = Content.Load<Effect>("RadialEffect");
        shadowTex = Content.Load<Texture2D>("shadow");
        Texture2D tileset = Game1.Instance.Content.Load<Texture2D>("tiles2");
        Texture2D objsImage = Content.Load<Texture2D>("objects");

        int tilesize = 32;

        tileRegister = new TileRegister();
        var register = tileRegister.Register;
        Tile newTile(string name, int px, int py) {
            var t = tilesize;
            return register(new Tile(tileset, new(px * t, py * t, t, t), name));
        }

        register(new(tileset, new(0, 64, 2, 2), "debug")).SetMapColor(Color.Magenta);
        newTile("floor1", 0, 0).SetMapColor(Color.DarkGray);
        newTile("floor2", 1, 0).SetMapColor(Color.DarkGray);
        newTile("coper", 2, 0).SetMapColor(Color.Orange);
        newTile("wall", 3, 0).SetMapColor(Color.Gray).SetSolid().SetLightPassable(false);

        map = new Map().Load();

        map.AddObjectRel(new(0, 0), new Cog(objsImage));
        map.AddObjectRel(new(0, 0), new Lever(objsImage));
        map.AddObjectRel(new(1, 0), new Cog(objsImage));
        map.AddObjectRel(new(2, 1), new BigCog(objsImage));
        map.AddObjectRel(new(2, 1), new Lever(objsImage));
        map.AddObjectRel(new(2, 3), new BigCog(objsImage));
        map.AddObjectRel(new(3, 2), new Cog(objsImage));

        map.AddObjectRel(new(3, 0), new Cog(objsImage));
        map.AddObjectRel(new(4, 0), new Cog(objsImage));
        map.AddObjectRel(new(3, -1), new Cog(objsImage));
        map.AddObjectRel(new(4, -1), new Cog(objsImage));
        map.AddObjectRel(new(4, -1), new Lever(objsImage));

        Camera.Load();
        Camera.MoveHardTo(new Vector2(map.WorldWidth/2, map.WorldHeight/2));

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime) {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        map.Update(gameTime);

        Camera.ScaleIndependent(gameTime);
        Camera.MoveIndependent(gameTime, 500);

        base.Update(gameTime);
    }

    Rectangle mapLightningRect;
    protected void RuntimeLoadEffect() {
        Rectangle camRect = Camera.GetViewBounds(GraphicsDevice);
        Rectangle r = map.GetVisibleTileRect(camRect);
        int border = 1;
        r = new Rectangle(r.X-border, r.Y-border, r.Width+border*2, r.Height+border*2);

        var mapLightningOffset = map.GetPosAtTile(new Point(r.X, r.Y));
        mapLightningRect = new Rectangle(
            (int)mapLightningOffset.X,
            (int)mapLightningOffset.Y,
            r.Width * map.TileSize,
            r.Height * map.TileSize
        );

        map.BuildVisibleLightTexture(r);

        // bind texture to shader slot s0
        GraphicsDevice.Textures[0] = shadowTex;

        // shader params
        radialEffect.Parameters["TileSize"].SetValue(1f / (map.TileSize/Camera.Zoom));
        radialEffect.Parameters["Softness"].SetValue(2f);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Black);
        DrawBackground(Color.CornflowerBlue);

        if (map.InWorldZoom)
            RuntimeLoadEffect();

        _spriteBatch.Begin(transformMatrix: CameraTransform, samplerState: SamplerState.PointClamp);
        map.Draw(_spriteBatch);
        _spriteBatch.End();

        _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: CameraTransform, effect: radialEffect);
        if (map.InWorldZoom) {
            _spriteBatch.Draw(map.LightTexture, mapLightningRect, Color.White);
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
