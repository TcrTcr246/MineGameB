using GameTemplate;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGameB.Entities;
using MineGameB.Items;
using MineGameB.World;
using MineGameB.World.Objects;
using MineGameB.World.Tiles;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MineGameB.Scenes;
public class GameScene() : Scene("game") {
    public static Map CaveMap { get; set; }
    public static Map SurfaceMap { get; set; }
    public static Map LocalMap { get; set; }
    public static LoadingScreen LoadingScreen { get; set; }
    public static Inventory Inventory { get; set; }

    public static TileRegister TileRegister { get; set; }

    static Effects.ShadowEffect shadowEffect;
    bool useShadowEffect = true;
    bool useCheats = true;
    private bool isMapReady = false;

    static ContentManager Content => Game1.Instance.Content;
    static Camera2D Camera => Game1.Instance.Camera;
    static Game1 Game1Inst => Game1.Instance;

    MouseState ms, lms;

    public static Player Player { get; set; } = new();

    public override void Load() {
        ms = Mouse.GetState();
        lms = ms;

        TileRegister = new TileRegister();
        TileDeclaratioions.Declare();

        CaveMap = new Map();
        SurfaceMap = new Map();

        var font = Content.Load<SpriteFont>("Font");
        var inventoryTexture = Content.Load<Texture2D>("inventory");

        Inventory = new(Game1Inst.GraphicsDevice, font, inventoryTexture);

        if (useCheats) {
            var tile2 = TileRegister.GetTileByName("debug");
            for (int i = 0; 15 > i; i++)
                Inventory.AddItem(tile2.Texture, tile2.SourceRectangle, "d" + i, 1);

            foreach (var t in new string[] { "mountain", "highMountain", "ultraHighMountain", "ultraRock", "wall" }) {
                var tile = TileRegister.GetTileByName(t);
                Inventory.AddItem(tile.Texture, tile.SourceRectangle, tile.Name, 99);
            }

            for (int i = 0; 15 > i; i++)
                Inventory.GetSlot(i).Count = 0;
        }


        LoadingScreen = new LoadingScreen(font);

        Generator.OnProgressUpdate += (progress, message) => {
            LoadingScreen.Progress = progress;
            LoadingScreen.Message = message;
        };

        Generator.OnProgressPhaseUpdate += (progress, message) => {
            LoadingScreen.PhaseProgress = progress;
            LoadingScreen.PhaseMessage = message;
        };

        LoadingScreen.IsVisible = true;

        Task.Run(async () => {
            var OnProgressUpdate = Generator.ReportProgress;
            var OnProgressPhaseUpdate = Generator.ReportPhaseProgress;

            OnProgressPhaseUpdate?.Invoke(0.0f, "Surface map generation... (1/3)");
            OnProgressUpdate?.Invoke(0.0f, "Initializing...");
            var surfaceTiles = await SurfaceMap.Generator.GenerateTopograficMapAsync();

            OnProgressPhaseUpdate?.Invoke(0.5f, "Cave map generation... (2/3)");
            OnProgressUpdate?.Invoke(0.0f, "Initializing...");
            var caveTiles = await CaveMap.Generator.GenerateWallAnd2FloorVariant("wall", "floor1", "floor2");

            OnProgressPhaseUpdate?.Invoke(1f, "Finishing...  (3/3)");

            OnProgressUpdate?.Invoke(0.0f, "Processing terrain data...");
            SurfaceMap.NewGenerate(surfaceTiles);
            CaveMap.NewGenerate(caveTiles);

            // Phase 3: Generate map texture (85-98%)
            OnProgressUpdate?.Invoke(0.75f, "Generating surface map texture...");
            SurfaceMap.GenTex();
            OnProgressUpdate?.Invoke(0.9f, "Generating cave map texture...");
            CaveMap.GenTex();

            OnProgressUpdate?.Invoke(1.0f, "Done!");
            OnProgressPhaseUpdate?.Invoke(1.0f, "Done!");

            await Task.Delay(1000);
            LoadingScreen.IsVisible = false;
            isMapReady = true;
        });

        LocalMap = SurfaceMap;

        // AddGears(objsImage, CaveMap);

        // Shadow effect and player initialization will happen after map generation

        base.Load();
    }

    bool wasMapReady = false;
    public override void Update(GameTime gameTime) {
        LoadingScreen.Update(gameTime);
        bool isMapJustReady = !wasMapReady && isMapReady;
        wasMapReady = isMapReady;

        if (!isMapReady) {
            base.Update(gameTime);
            return;
        }

        // Initialize shadow effect once map is ready
        if (useShadowEffect && shadowEffect == null) {
            shadowEffect = new(Content.Load<Effect>("RadialEffect"), LocalMap, Content.Load<Texture2D>("shadow"));
        }

        // Set player position on first frame after generation
        if (isMapJustReady) {
            Debug.WriteLine("Map is ready.");
            Player.SetPosition(new Vector2(LocalMap.WorldWidth / 2, LocalMap.WorldHeight / 2));
            Camera.MoveHardTo(Player.Center.ToVector2());
        }

        LocalMap.Update(gameTime);

        lms = ms;
        ms = Mouse.GetState();

        var LS = Keyboard.GetState().IsKeyDown(Keys.LeftShift);
        var LC = Keyboard.GetState().IsKeyDown(Keys.LeftControl);
        var LA = Keyboard.GetState().IsKeyDown(Keys.LeftAlt);
        var F = Keyboard.GetState().IsKeyDown(Keys.F);

        if (LS && useCheats) {
            Camera.ScaleIndependent(gameTime, 100f, 7f, 0.008f);
            Camera.MoveIndependent(gameTime, LC ? 20000 : 3000);
            if (F)
                Player.SetPosition(Camera.MouseWorld);
        } else {
            Camera.ScaleIndependent(gameTime, 100f, 7f, 0.9f);
            Camera.MoveTo(gameTime, Player.Center.ToVector2());
        }

        if (LocalMap.InWorldZoom && !Inventory.IsMouseUsed()) {
            Point loc = LocalMap.GetIndexAtPos(Game1.Instance.Camera.MouseWorld);
            Tile tile = LocalMap.GetTileObjectAtIndex(loc);

            if (ms.LeftButton == ButtonState.Pressed) {
                LocalMap.BreakTileAtIndex(gameTime, loc, (LA && useCheats) ? 100f : 1f);
            }

            if (ms.RightButton == ButtonState.Pressed) {
                if (!LocalMap.GetTileObjectAtIndex(loc).IsSolid) {
                    var slot = Inventory.GetSelectedHotbarSlot();
                    if (!slot.IsEmpty) {
                        LocalMap.SetTileAtIndex(loc, TileRegister.GetIdByName(slot.ItemName));
                        slot.Count -= 1;
                    }
                }
            }
        }

        Player.Update(gameTime, LocalMap);
        Inventory.Update(gameTime);

        if (useShadowEffect)
            shadowEffect.Update(gameTime);

        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch) {
        Game1Inst.DrawBackground(Color.CornflowerBlue);

        if (!isMapReady) {
            LoadingScreen.Draw(spriteBatch);
            base.Draw(spriteBatch);
            return;
        }

        spriteBatch.Begin(transformMatrix: Game1Inst.CameraTransform, samplerState: SamplerState.PointClamp);
        LocalMap.Draw(spriteBatch);
        Player.Draw(spriteBatch);
        spriteBatch.End();

        if (useShadowEffect)
            shadowEffect.Draw(spriteBatch);

        spriteBatch.Begin(transformMatrix: Game1Inst.LetterboxUITransform, samplerState: SamplerState.PointClamp);
        Inventory.Draw(spriteBatch);
        spriteBatch.End();

        LoadingScreen.Draw(spriteBatch);
        base.Draw(spriteBatch);
    }
}