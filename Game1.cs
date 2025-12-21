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
        Camera.TranslateBackToWorldPos = false;
        base.Initialize();
    }

    Map map;
    Texture2D dotTex;
    public TileRegister tileRegister;
    Effects.ShadowEffect shadowEffect;

    public Game1() : base() {
        Instance = this;
    }

    protected override void LoadContent() {
        dotTex = new Texture2D(GraphicsDevice, 1, 1);
        dotTex.SetData([Color.White]);

        Texture2D tileset = Game1.Instance.Content.Load<Texture2D>("tiles2");
        Texture2D objsImage = Content.Load<Texture2D>("objects");

        int tilesize = 32;

        tileRegister = new TileRegister();
        var register = tileRegister.Register;
        Tile newTile(string name, int px, int py) {
            var t = tilesize;
            return register(new Tile(tileset, new(px * t, py * t, t, t), name));
        }

        register(new(tileset, new(0, 64, 32, 32), "debug")).SetMapColor(Color.Magenta);
        register(new(tileset, new(32, 64, 32, 32), "blank_white")).SetMapColor(Color.White).SetDrawColor(Color.White);
        register(new(tileset, new(32, 64, 32, 32), "blank_blue")).SetMapColor(Color.AliceBlue).SetDrawColor(Color.AliceBlue);
        newTile("floor1", 0, 0).SetMapColor(Color.DarkGray);
        newTile("floor2", 1, 0).SetMapColor(Color.DarkGray);
        newTile("coper", 2, 0).SetMapColor(Color.Orange);
        newTile("wall", 3, 0).SetMapColor(Color.Gray).SetSolid().SetLightPassable(false);

        map = new Map().Load()
            .NewGenerate((x, y) => {
                return tileRegister.GetIdByName((x+y) % 2 == 0 ? "blank_white" : "blank_blue");
            });

        map.AddObjectRel(new(-4, 0), new Cog(objsImage));
        map.AddObjectRel(new(-4, 0), new Rotator(objsImage));
        map.AddObjectRel(new(-3, 0), new Cog(objsImage));
        map.AddObjectRel(new(-2, 0), new Cog(objsImage));
        map.AddObjectRel(new(-1, 0), new Cog(objsImage));
        map.AddObjectRel(new(0, 0), new Cog(objsImage));
        map.AddObjectRel(new(0, 0), new Lever(objsImage));
        map.AddObjectRel(new(1, 0), new Cog(objsImage));
        map.AddObjectRel(new(2, 1), new BigCog(objsImage));
        map.AddObjectRel(new(2, 1), new Lever(objsImage));
        map.AddObjectRel(new(2, 3), new BigCog(objsImage));
        map.AddObjectRel(new(3, 2), new Cog(objsImage));

        map.AddObjectRel(new(3, 4), new Cog(objsImage));
        map.AddObjectRel(new(4, 5), new BigCog(objsImage));
        map.AddObjectRel(new(3, 6), new Cog(objsImage));
        map.AddObjectRel(new(4, 7), new BigCog(objsImage));
        map.AddObjectRel(new(3, 8), new Cog(objsImage));
        map.AddObjectRel(new(4, 9), new BigCog(objsImage));

        map.AddObjectRel(new(3, 0), new Cog(objsImage));
        map.AddObjectRel(new(4, 0), new Cog(objsImage));
        map.AddObjectRel(new(3, -1), new Cog(objsImage));
        map.AddObjectRel(new(4, -1), new Cog(objsImage));
        map.AddObjectRel(new(4, -1), new Lever(objsImage));


        var shadowTex = Content.Load<Texture2D>("shadow");
        shadowEffect = new(Content.Load<Effect>("RadialEffect"), map, shadowTex);

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

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Black);
        DrawBackground(Color.CornflowerBlue);

        _spriteBatch.Begin(transformMatrix: CameraTransform, samplerState: SamplerState.PointClamp);
        map.Draw(_spriteBatch);
        _spriteBatch.End();

        shadowEffect.Draw(_spriteBatch);

        base.Draw(gameTime);
    }
}
