using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameTemplate;
public class Game : Microsoft.Xna.Framework.Game {
    protected GraphicsDeviceManager _graphics;
    protected SpriteBatch _spriteBatch;

    public Game() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;

        Camera = new();
        Camera.MoveHardTo(new Vector2(0, 0));
    }

    protected override void Initialize() {
        Letterbox.Initialize(GraphicsDevice, Window);
        Letterbox.UpdateScaleMatrix(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        base.Initialize();
    }


    public Camera2D Camera;
    public Matrix CameraTransform, LetterboxUITransform;

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime) {
        Camera.Update(gameTime);
        Letterbox.UpdateScaleMatrix(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        CameraTransform = Camera.GetTransform();
        LetterboxUITransform = Camera2D.GetTransformUnpositioned(GraphicsDevice);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        base.Draw(gameTime);
    }

    public static void DrawFillRect(GraphicsDevice gd, Vector2 topLeft, float width, float height, Color color, Matrix? transform = null) {
        VertexPositionColor[] vertices = new VertexPositionColor[6];

        Vector3 tl = new(topLeft, 0);
        Vector3 tr = new(topLeft + new Vector2(width, 0), 0);
        Vector3 br = new(topLeft + new Vector2(width, height), 0);
        Vector3 bl = new(topLeft + new Vector2(0, height), 0);

        // două triunghiuri
        vertices[0] = new VertexPositionColor(tl, color);
        vertices[1] = new VertexPositionColor(tr, color);
        vertices[2] = new VertexPositionColor(br, color);

        vertices[3] = new VertexPositionColor(tl, color);
        vertices[4] = new VertexPositionColor(br, color);
        vertices[5] = new VertexPositionColor(bl, color);

        BasicEffect effect = new(gd) {
            VertexColorEnabled = true,
            World = transform ?? Matrix.Identity,
            View = Matrix.Identity,
            Projection = Matrix.CreateOrthographicOffCenter(0, gd.Viewport.Width, gd.Viewport.Height, 0, 0, 1)
        };

        foreach (var pass in effect.CurrentTechnique.Passes) {
            pass.Apply();
            gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
        }
    }

    public void DrawBackground(Color color) {
        DrawFillRect(GraphicsDevice, new(0, 0), Letterbox.ScreenWidth, Letterbox.ScreenHeight, color);
    }
}
