using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGameB.Scenes;
using MineGameB.World;

namespace MineGameB;
public class Game1 : GameTemplate.Game {
    public static Game1 Instance { get; private set; }
    public static Texture2D Pixel { get; private set; }

    protected override void Initialize() {
        Camera.TranslateBackToWorldPos = false;

        /*/
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.ApplyChanges();
        //*/

        base.Initialize();
    }

    public Game1() : base() {
        Instance = this;
    }

    protected override void LoadContent() {
        Pixel = new Texture2D(GraphicsDevice, 1, 1);
        Pixel.SetData([Color.White]);

        Scene.AddScene(new GameScene());

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime) {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Scene.GetCurentScene().Update(gameTime);

        float fps = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
        Window.Title = $"MineGame - FPS: {fps:0.0} - seed: {GameScene.LocalMap.GetSeed()}";

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Black);

        Scene.GetCurentScene().Draw(_spriteBatch);

        base.Draw(gameTime);
    }
}
