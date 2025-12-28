using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineGameB.World;

namespace MineGameB.Entities;
public class Player {
    public Vector2 Position;
    public float Speed = 160f;

    public const int Size = 32;

    public Rectangle Bounds => new(
        (int)Position.X,
        (int)Position.Y,
        Size,
        Size
    );

    public Point Center => new(
        (int)(Position.X + Size / 2),
        (int)(Position.Y + Size / 2)
    );

    public void SetPosition(Vector2 pos) {
        Position = pos;
    }

    public void Update(GameTime gameTime, Map world) {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 move = Vector2.Zero;
        var k = Keyboard.GetState();

        if (k.IsKeyDown(Keys.W))
            move.Y -= 1;
        if (k.IsKeyDown(Keys.S))
            move.Y += 1;
        if (k.IsKeyDown(Keys.A))
            move.X -= 1;
        if (k.IsKeyDown(Keys.D))
            move.X += 1;

        if (move != Vector2.Zero)
            move.Normalize();

        // X axis
        Position.X += move.X * Speed * dt;
        if (world.TouchesAnySolidTile(Bounds))
            Position.X -= move.X * Speed * dt;

        // Y axis
        Position.Y += move.Y * Speed * dt;
        if (world.TouchesAnySolidTile(Bounds))
            Position.Y -= move.Y * Speed * dt;
    }

    public void Draw(SpriteBatch spriteBatch) {
        spriteBatch.Draw(
            Game1.Pixel,
            Bounds,
            Color.Red
        );
    }
}
