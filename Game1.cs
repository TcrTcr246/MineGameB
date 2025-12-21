using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGameB.Scenes;

namespace MineGameB;
public class Game1 : GameTemplate.Game {
    public static Game1 Instance { get; private set; }
    Texture2D dotTex;

    protected override void Initialize() {
        Camera.TranslateBackToWorldPos = false;
        base.Initialize();
    }

    public Game1() : base() {
        Instance = this;
    }

    protected override void LoadContent() {
        dotTex = new Texture2D(GraphicsDevice, 1, 1);
        dotTex.SetData([Color.White]);

        Scene.AddScene(new GameScene());

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime) {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Scene.GetCurentScene().Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Black);

        Scene.GetCurentScene().Draw(_spriteBatch);

        base.Draw(gameTime);
    }
}
