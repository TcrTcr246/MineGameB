using GameTemplate;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGameB.Entities;
using MineGameB.World;
using MineGameB.World.Objects;
using MineGameB.World.Tiles;
using System;
using System.Linq;

namespace MineGameB.Scenes;
public class GameScene() : Scene("game") {
    public static Map CaveMap { get; set; }
    public static Map SurfaceMap { get; set; }
    public static Map LocalMap { get; set; }
    public static TileRegister TileRegister { get; set; }

    static Effects.ShadowEffect shadowEffect;
    const bool useShadowEffect = true;

    static ContentManager Content => Game1.Instance.Content;
    static Camera2D Camera => Game1.Instance.Camera;
    static Game1 Game1Inst => Game1.Instance;

    MouseState ms, lms;

    public static Player Player = new();

#pragma warning disable IDE0079
#pragma warning disable CA1822
    public void AddGears(Texture2D objsImage, Map map) {
        map.TranslateAddObject(new(0, -4));
        map.AddObjectRel(new(-4, 0), new Cog(objsImage));
        map.AddObjectRel(new(-4, 0), new Lever(objsImage));
        map.AddObjectRel(new(-3, 0), new Cog(objsImage));
        map.AddObjectRel(new(-3, 0), new Lock(objsImage));

        map.AddObjectRel(new(0, -1), new Cog(objsImage));
        map.AddObjectRel(new(0, -1), new Lock(objsImage));
        map.AddObjectRel(new(0, -2), new Cog(objsImage));
        map.AddObjectRel(new(0, -2), new Motor(objsImage));

        map.AddObjectRel(new(-2, 0), new Cog(objsImage));
        map.AddObjectRel(new(-1, 0), new Cog(objsImage));
        map.AddObjectRel(new(0, 0), new Cog(objsImage));
        map.AddObjectRel(new(1, 0), new Cog(objsImage));

        map.AddObjectRel(new(2, 1), new BigCog(objsImage));
        map.AddObjectRel(new(2, 1), new Lever(objsImage));
        map.AddObjectRel(new(2, 3), new BigCog(objsImage));
        //map.AddObjectRel(new(2, 3), new Motor(objsImage));
        map.AddObjectRel(new(3, 2), new Cog(objsImage));

        for (int i = 0; i < 6; i++) {
            map.AddObjectRel(new(-4 - i, 5), new Cog(objsImage));
            if (i % 2 == 0)
                map.AddObjectRel(new(-4 - i, 5), new Lock(objsImage));
            else
                map.AddObjectRel(new(-4 - i, 5), new Motor(objsImage));
        }

        map.AddObjectRel(new(-3, 5), new Cog(objsImage));
        map.AddObjectRel(new(-2, 5), new Cog(objsImage));
        map.AddObjectRel(new(-1, 5), new Cog(objsImage));
        map.AddObjectRel(new(0, 5), new Cog(objsImage));
        map.AddObjectRel(new(-1, 7), new BigCog(objsImage));
        map.AddObjectRel(new(-2, 8), new Cog(objsImage));
        map.AddObjectRel(new(-1, 9), new BigCog(objsImage));
        // map.AddObjectRel(new(-1, 9), new Rotator(objsImage));
        map.AddObjectRel(new(0, 6), new Cog(objsImage));
        map.AddObjectRel(new(1, 6), new Cog(objsImage));
        map.AddObjectRel(new(2, 6), new Cog(objsImage));

        map.AddObjectRel(new(3, 4), new Cog(objsImage));
        map.AddObjectRel(new(4, 5), new BigCog(objsImage));
        map.AddObjectRel(new(4, 5), new Lock(objsImage));
        map.AddObjectRel(new(3, 6), new Cog(objsImage));
        map.AddObjectRel(new(4, 7), new BigCog(objsImage));
        map.AddObjectRel(new(3, 8), new Cog(objsImage));
        map.AddObjectRel(new(3, 8), new Lever(objsImage));
        map.AddObjectRel(new(4, 9), new BigCog(objsImage));
        map.AddObjectRel(new(4, 9), new Lever(objsImage));

        map.AddObjectRel(new(3, 0), new Cog(objsImage));
        map.AddObjectRel(new(4, 0), new Cog(objsImage));
        map.AddObjectRel(new(3, -1), new Cog(objsImage));
        map.AddObjectRel(new(4, -1), new Cog(objsImage));
        map.AddObjectRel(new(4, -1), new Lever(objsImage));
    }
#pragma warning restore CA1822
#pragma warning restore IDE0079

    public override void Load() {
        ms = Mouse.GetState();
        lms = ms;

        Texture2D CaveTileset = Content.Load<Texture2D>("CaveTiles");
        Texture2D SurfaceTileset = Content.Load<Texture2D>("NatureTiles");
        Texture2D objsImage = Content.Load<Texture2D>("Objects");

        int tilesize = 32;

        TileRegister = new TileRegister();
        var register = TileRegister.Register;
        Tile newTile(Texture2D tileset, string name, int px, int py) {
            var t = tilesize;
            return register(new Tile(tileset, new(px * t, py * t, t, t), name));
        }

        register(new(CaveTileset, new(0, 64, 32, 32), "debug")).SetMapColor(Color.Magenta);
        register(new(CaveTileset, new(32, 64, 32, 32), "blank_white")).SetMapColor(Color.White).SetDrawColor(Color.White);
        register(new(CaveTileset, new(32, 64, 32, 32), "blank_blue")).SetMapColor(Color.AliceBlue).SetDrawColor(Color.AliceBlue);

        newTile(CaveTileset, "floor1", 0, 0).SetMapColor(Color.DarkGray);
        newTile(CaveTileset, "floor2", 1, 0).SetMapColor(Color.DarkGray);
        newTile(CaveTileset, "wall", 3, 0).SetMapColor(Color.Gray).SetSolid().SetDurity(150f).SetLightPassable(false);

        newTile(SurfaceTileset, "grassVar1", 0, 0).SetMapColor(Color.LightGreen);
        newTile(SurfaceTileset, "grassVar2", 1, 0).SetMapColor(Color.LightGreen);
        newTile(SurfaceTileset, "grassVar3", 2, 0).SetMapColor(Color.LightGreen);
        newTile(SurfaceTileset, "grassVar4", 3, 0).SetMapColor(Color.LightGreen);
        newTile(SurfaceTileset, "forestVar1", 0, 1).SetMapColor(Color.Green);
        newTile(SurfaceTileset, "forestVar2", 1, 1).SetMapColor(Color.Green);
        newTile(SurfaceTileset, "forestVar3", 2, 1).SetMapColor(Color.Green);
        newTile(SurfaceTileset, "sandVar1", 0, 2).SetMapColor(Color.LightGoldenrodYellow);
        newTile(SurfaceTileset, "sandVar2", 1, 2).SetMapColor(Color.LightGoldenrodYellow);
        newTile(SurfaceTileset, "sandVar3", 2, 2).SetMapColor(Color.LightGoldenrodYellow);
        newTile(SurfaceTileset, "water", 0, 3).SetMapColor(Color.LightBlue).SetSolid();
        newTile(SurfaceTileset, "mountain", 0, 4).SetMapColor(Color.LightGray).SetSolid().SetDurity(150f).SetLightPassable(false);
        newTile(SurfaceTileset, "highMountain", 1, 4).SetMapColor(Color.DarkGray).SetSolid().SetDurity(450f).SetLightPassable(false);


        CaveMap = new Map();
        CaveMap.NewGenerate(CaveMap.Generator.GenerateWallAnd2FloorVariant("wall", "floor1", "floor2").Tiles);

        SurfaceMap = new Map();
        SurfaceMap.NewGenerate(SurfaceMap.Generator.GenerateTopograficMap());

        LocalMap = SurfaceMap;

        // AddGears(objsImage, CaveMap);

        if (useShadowEffect)
            shadowEffect = new(Content.Load<Effect>("RadialEffect"), LocalMap, Content.Load<Texture2D>("shadow"));

        Player.SetPosition(new Vector2(LocalMap.WorldWidth / 2, LocalMap.WorldHeight / 2));
        Camera.MoveHardTo(Player.Center.ToVector2());

        base.Load();
    }

    int frameCount = 0;
    public override void Update(GameTime gameTime) {
        LocalMap.Update(gameTime);

        lms = ms;
        ms = Mouse.GetState();

        Camera.ScaleIndependent(gameTime);
        Camera.MoveTo(gameTime, Player.Center.ToVector2());

        /*
        var LS = Keyboard.GetState().IsKeyDown(Keys.LeftShift);
        var LC = Keyboard.GetState().IsKeyDown(Keys.LeftControl);
        Camera.MoveIndependent(gameTime, (!LS ? 500 : 5000) + (!LC ? 0 : 10000));
        */

        if (LocalMap.InWorldZoom) {
            Point loc = LocalMap.GetIndexAtPos(Camera2D.MouseWorld);
            Tile tile = LocalMap.GetTileObjectAtIndex(loc);

            if (ms.LeftButton == ButtonState.Pressed) {
                LocalMap.BreakTileAtIndex(loc, gameTime);
            }

            if (ms.RightButton == ButtonState.Pressed) {
                if (!LocalMap.GetTileObjectAtIndex(loc).IsSolid)
                    LocalMap.SetTileAtIndex(loc, TileRegister.GetIdByName("wall"));
            }
        }

        Player.Update(gameTime, LocalMap);
        if (useShadowEffect)
            shadowEffect.Update(gameTime);

        frameCount++;
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch) {
        Game1Inst.DrawBackground(Color.CornflowerBlue);

        spriteBatch.Begin(transformMatrix: Game1Inst.CameraTransform, samplerState: SamplerState.PointClamp);
        LocalMap.Draw(spriteBatch);
        Player.Draw(spriteBatch);
        spriteBatch.End();

        if (useShadowEffect)
            shadowEffect.Draw(spriteBatch);

        base.Draw(spriteBatch);
    }
}
