using GameTemplate;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGameB.World;
using MineGameB.World.Objects;
using MineGameB.World.Tiles;

namespace MineGameB.Scenes;
public class GameScene() : Scene("game") {
    public static Map LocalMap { get; set; }
    public static TileRegister TileRegister { get; set; }

    static Effects.ShadowEffect shadowEffect;

    static ContentManager Content => Game1.Instance.Content;
    static Camera2D Camera => Game1.Instance.Camera;
    static Game1 Game1Inst => Game1.Instance;

    MouseState ms, lms;

#pragma warning disable IDE0079
#pragma warning disable CA1822
    public void AddGears(Texture2D objsImage) {
        LocalMap.TranslateAddObject(new(0, -4));
        LocalMap.AddObjectRel(new(-4, 0), new Cog(objsImage));
        LocalMap.AddObjectRel(new(-4, 0), new Lever(objsImage));
        LocalMap.AddObjectRel(new(-3, 0), new Cog(objsImage));
        LocalMap.AddObjectRel(new(-3, 0), new Lock(objsImage));

        LocalMap.AddObjectRel(new(0, -1), new Cog(objsImage));
        LocalMap.AddObjectRel(new(0, -1), new Lock(objsImage));
        LocalMap.AddObjectRel(new(0, -2), new Cog(objsImage));
        LocalMap.AddObjectRel(new(0, -2), new Motor(objsImage));

        LocalMap.AddObjectRel(new(-2, 0), new Cog(objsImage));
        LocalMap.AddObjectRel(new(-1, 0), new Cog(objsImage));
        LocalMap.AddObjectRel(new(0, 0), new Cog(objsImage));
        LocalMap.AddObjectRel(new(1, 0), new Cog(objsImage));

        LocalMap.AddObjectRel(new(2, 1), new BigCog(objsImage));
        LocalMap.AddObjectRel(new(2, 1), new Lever(objsImage));
        LocalMap.AddObjectRel(new(2, 3), new BigCog(objsImage));
        //map.AddObjectRel(new(2, 3), new Motor(objsImage));
        LocalMap.AddObjectRel(new(3, 2), new Cog(objsImage));

        for (int i = 0; i < 6; i++) {
            LocalMap.AddObjectRel(new(-4 - i, 5), new Cog(objsImage));
            if (i % 2 == 0)
                LocalMap.AddObjectRel(new(-4 - i, 5), new Lock(objsImage));
            else
                LocalMap.AddObjectRel(new(-4 - i, 5), new Motor(objsImage));
        }

        LocalMap.AddObjectRel(new(-3, 5), new Cog(objsImage));
        LocalMap.AddObjectRel(new(-2, 5), new Cog(objsImage));
        LocalMap.AddObjectRel(new(-1, 5), new Cog(objsImage));
        LocalMap.AddObjectRel(new(0, 5), new Cog(objsImage));
        LocalMap.AddObjectRel(new(-1, 7), new BigCog(objsImage));
        LocalMap.AddObjectRel(new(-2, 8), new Cog(objsImage));
        LocalMap.AddObjectRel(new(-1, 9), new BigCog(objsImage));
        // map.AddObjectRel(new(-1, 9), new Rotator(objsImage));
        LocalMap.AddObjectRel(new(0, 6), new Cog(objsImage));
        LocalMap.AddObjectRel(new(1, 6), new Cog(objsImage));
        LocalMap.AddObjectRel(new(2, 6), new Cog(objsImage));

        LocalMap.AddObjectRel(new(3, 4), new Cog(objsImage));
        LocalMap.AddObjectRel(new(4, 5), new BigCog(objsImage));
        LocalMap.AddObjectRel(new(4, 5), new Lock(objsImage));
        LocalMap.AddObjectRel(new(3, 6), new Cog(objsImage));
        LocalMap.AddObjectRel(new(4, 7), new BigCog(objsImage));
        LocalMap.AddObjectRel(new(3, 8), new Cog(objsImage));
        LocalMap.AddObjectRel(new(3, 8), new Lever(objsImage));
        LocalMap.AddObjectRel(new(4, 9), new BigCog(objsImage));
        LocalMap.AddObjectRel(new(4, 9), new Lever(objsImage));

        LocalMap.AddObjectRel(new(3, 0), new Cog(objsImage));
        LocalMap.AddObjectRel(new(4, 0), new Cog(objsImage));
        LocalMap.AddObjectRel(new(3, -1), new Cog(objsImage));
        LocalMap.AddObjectRel(new(4, -1), new Cog(objsImage));
        LocalMap.AddObjectRel(new(4, -1), new Lever(objsImage));
    }
#pragma warning restore CA1822
#pragma warning restore IDE0079

    public override void Load() {
        Texture2D tileset = Content.Load<Texture2D>("tiles2");
        Texture2D objsImage = Content.Load<Texture2D>("objects");
        _ = objsImage;

        ms = Mouse.GetState();
        lms = ms;

        int tilesize = 32;

        TileRegister = new TileRegister();
        var register = TileRegister.Register;
        Tile newTile(string name, int px, int py) {
            var t = tilesize;
            return register(new Tile(tileset, new(px * t, py * t, t, t), name));
        }

        register(new(tileset, new(0, 64, 32, 32), "debug")).SetMapColor(Color.Magenta);
        register(new(tileset, new(32, 64, 32, 32), "blank_white")).SetMapColor(Color.White).SetDrawColor(Color.White);
        register(new(tileset, new(32, 64, 32, 32), "blank_blue")).SetMapColor(Color.AliceBlue).SetDrawColor(Color.AliceBlue);
        newTile("floor1", 0, 0).SetMapColor(Color.DarkGray);
        newTile("floor2", 1, 0).SetMapColor(Color.DarkGray);
        newTile("wall", 3, 0).SetMapColor(Color.Gray).SetSolid().SetLightPassable(false).SetDurity(4f);

        LocalMap = new Map().Load();
        /* LocalMap.NewGenerate((x, y) => {
            return TileRegister.GetIdByName((x + y) % 2 == 0 ? "blank_white" : "blank_blue");
        }); */
        LocalMap.Generate();

        // AddGears(objsImage);

        var shadowTex = Content.Load<Texture2D>("shadow");
        shadowEffect = new(Content.Load<Effect>("RadialEffect"), LocalMap, shadowTex);

        Camera.Load();
        Camera.MoveHardTo(new Vector2(LocalMap.WorldWidth / 2, LocalMap.WorldHeight / 2));

        base.Load();
    }

    public override void Update(GameTime gameTime) {
        LocalMap.Update(gameTime);

        lms = ms;
        ms = Mouse.GetState();

        Camera.ScaleIndependent(gameTime);
        var LS = Keyboard.GetState().IsKeyDown(Keys.LeftShift);
        var LC = Keyboard.GetState().IsKeyDown(Keys.LeftControl);
        Camera.MoveIndependent(gameTime, (!LS ? 500 : 5000) + (!LC ? 0 : 10000));

        if (LocalMap.InWorldZoom) {
            Point loc = LocalMap.GetIndexAtPos(Camera2D.MouseWorld);
            Tile tile = LocalMap.GetTileObjectAtIndex(loc);

            if (tile.IsBreakable && ms.LeftButton == ButtonState.Pressed) {
                if (LocalMap.GetTileNameAtIndex(loc) == "wall")
                    LocalMap.RemoveTileAtIndex(loc);
            }

            if (ms.RightButton == ButtonState.Pressed) {
                if (!LocalMap.GetTileObjectAtIndex(loc).IsSolid)
                    LocalMap.SetTileAtIndex(loc, TileRegister.GetIdByName("wall"));
            }
        }

        shadowEffect.Update(gameTime);

        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch) {
        Game1Inst.DrawBackground(Color.CornflowerBlue);

        spriteBatch.Begin(transformMatrix: Game1Inst.CameraTransform, samplerState: SamplerState.PointClamp);
        LocalMap.Draw(spriteBatch);
        spriteBatch.End();

        shadowEffect.Draw(spriteBatch);

        base.Draw(spriteBatch);
    }
}
