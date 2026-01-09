using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MineGameB.Scenes;
using MineGameB.World.Objects;
using MineGameB.World.Tiles;
using System;

namespace MineGameB.World;
public static class TileDeclaratioions {
    const int tilesize = 32;
    static ContentManager Content => Game1.Instance.Content;
    static TileRegister TileRegister => GameScene.TileRegister;

    public static void Declare() {
        Texture2D CaveTileset = Content.Load<Texture2D>("CaveTiles");
        Texture2D SurfaceTileset = Content.Load<Texture2D>("NatureTiles");
        Texture2D objsImage = Content.Load<Texture2D>("Objects");

        var register = TileRegister.Register;
        Tile newTile(Texture2D tileset, string name, int px, int py) {
            var t = tilesize;
            return register(new Tile(tileset, new(px * t, py * t, t, t), name));
        }

        Random TileCoveredRandomizer = new();
        int randSand(int myId, int overId) => TileRegister.GetIdByName("sandVar1") + TileCoveredRandomizer.Next(0, 1);
        int randGrass(int myId, int overId) => TileRegister.GetIdByName("grassVar6");

        register(new(CaveTileset, new(0, 64, 32, 32), "debug")).SetMapColor(Color.Magenta);
        register(new(CaveTileset, new(32, 64, 32, 32), "blank_white")).SetMapColor(Color.White).SetDrawColor(Color.White);
        register(new(CaveTileset, new(32, 64, 32, 32), "blank_blue")).SetMapColor(Color.AliceBlue).SetDrawColor(Color.AliceBlue);

        newTile(CaveTileset, "floor1", 0, 0).SetMapColor(Color.DarkGray);
        newTile(CaveTileset, "floor2", 1, 0).SetMapColor(Color.DarkGray);
        newTile(CaveTileset, "wall", 3, 0).SetMapColor(Color.Gray).SetSolid().SetDurity(150f).SetLightPassable(false);

        newTile(SurfaceTileset, "grassVar1", 0, 0).SetMapColor(Color.LightGreen).SetTransformIntoAfterCover(randGrass);
        newTile(SurfaceTileset, "grassVar2", 1, 0).SetMapColor(Color.LightGreen).SetTransformIntoAfterCover(randGrass);
        newTile(SurfaceTileset, "grassVar3", 2, 0).SetMapColor(Color.LightGreen).SetTransformIntoAfterCover(randGrass);
        newTile(SurfaceTileset, "grassVar4", 3, 0).SetMapColor(Color.LightGreen);
        newTile(SurfaceTileset, "grassVar5", 4, 0).SetMapColor(Color.LightGreen);
        newTile(SurfaceTileset, "grassVar6", 5, 0).SetMapColor(Color.LightGreen);

        newTile(SurfaceTileset, "forestVar1", 0, 1).SetMapColor(Color.Green);
        newTile(SurfaceTileset, "forestVar2", 1, 1).SetMapColor(Color.Green);
        newTile(SurfaceTileset, "forestVar3", 2, 1).SetMapColor(Color.Green);

        var yellow = new Color(250, 255, 160);
        newTile(SurfaceTileset, "water", 0, 3).SetMapColor(Color.LightBlue).SetSolid();
        newTile(SurfaceTileset, "sandVar1", 0, 2).SetMapColor(yellow);
        newTile(SurfaceTileset, "sandVar2", 1, 2).SetMapColor(yellow);
        newTile(SurfaceTileset, "sandVar3", 2, 2).SetMapColor(yellow).SetTransformIntoAfterCover(randSand);
        newTile(SurfaceTileset, "sandVar4", 3, 2).SetMapColor(yellow).SetTransformIntoAfterCover(randSand);

        newTile(SurfaceTileset, "mountain", 0, 5).SetMapColor(Color./*LightGray*/DarkSlateGray).SetSolid().SetDurity(150f).SetLightPassable(false);
        newTile(SurfaceTileset, "highMountain", 1, 5).SetMapColor(Color./*DarkGray*/DarkSlateGray).SetSolid().SetDurity(700f).SetLightPassable(false);
        newTile(SurfaceTileset, "ultraHighMountain", 2, 5).SetMapColor(Color./*DarkSlateGray*/DarkSlateGray).SetSolid().SetDurity(3000f).SetLightPassable(false);
        newTile(SurfaceTileset, "ultraRock", 3, 4).SetMapColor(Color.Black).SetSolid().SetLightPassable(false);

        newTile(SurfaceTileset, "mountain_floor", 0, 6).SetMapColor(Color.DarkGray);
        newTile(SurfaceTileset, "highMountain_floor", 1, 6).SetMapColor(Color.Gray);
        newTile(SurfaceTileset, "ultraHighMountain_floor", 2, 6).SetMapColor(new Color(60, 60, 60));
    }

#pragma warning disable IDE0079
#pragma warning disable CA1822
    public static void AddGears(Texture2D objsImage, Map map) {
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
}
