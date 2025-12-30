using GameTemplate;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MineGameB.World;

public class LoadingScreen {
    private SpriteFont _font;
    private Texture2D _pixelTexture;

    public string Message { get; set; } = "Loading...";
    public float Progress { get; set; } = 0f; // 0.0 to 1.0
    public bool IsVisible { get; set; } = false;

    private float _spinnerRotation = 0f;
    private const float SPINNER_SPEED = 3f;

    public LoadingScreen(SpriteFont font) {
        _font = font;

        // Create a 1x1 white pixel texture for drawing shapes
        _pixelTexture = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _pixelTexture.SetData([Color.White]);
    }

    public void Update(GameTime gameTime) {
        if (IsVisible) {
            _spinnerRotation += SPINNER_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    SpriteBatch _spriteBatch;
    public void Draw(SpriteBatch _spriteBatch) {
        if (!IsVisible)
            return;
        this._spriteBatch = _spriteBatch;

        var viewport = Game1.Instance.Camera.Screen;
        int screenWidth = viewport.Width;
        int screenHeight = viewport.Height;

        _spriteBatch.Begin(transformMatrix:Game1.Instance.LetterboxUITransform);
        Game1.Instance.DrawBackground(Color.Black);

        // Semi-transparent background overlay
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(0, 0, screenWidth, screenHeight),
            Color.Black * 0.7f);

        // Progress bar dimensions
        int barWidth = 400;
        int barHeight = 30;
        int barX = (screenWidth - barWidth) / 2;
        int barY = screenHeight / 2 + 20;

        // Progress bar background
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(barX, barY, barWidth, barHeight),
            Color.DarkGray);

        // Progress bar fill
        int fillWidth = (int)(barWidth * Math.Clamp(Progress, 0f, 1f));
        _spriteBatch.Draw(_pixelTexture,
            new Rectangle(barX, barY, fillWidth, barHeight),
            Color.LimeGreen);

        // Progress bar border
        DrawRectangleBorder(barX, barY, barWidth, barHeight, 2, Color.White);

        // Loading message
        Vector2 messageSize = _font.MeasureString(Message);
        Vector2 messagePos = new Vector2(
            (screenWidth - messageSize.X) / 2,
            barY - messageSize.Y - 20);
        _spriteBatch.DrawString(_font, Message, messagePos, Color.White);

        // Percentage text
        string percentText = $"{(int)(Progress * 100)}%";
        Vector2 percentSize = _font.MeasureString(percentText);
        Vector2 percentPos = new Vector2(
            (screenWidth - percentSize.X) / 2,
            barY + barHeight + 10);
        _spriteBatch.DrawString(_font, percentText, percentPos, Color.White);

        // Spinning loading indicator
        DrawSpinner(screenWidth / 2, screenHeight / 2 - 80, 30, 6);

        _spriteBatch.End();
    }

    private void DrawSpinner(int centerX, int centerY, int radius, int dotCount) {
        float angleStep = MathHelper.TwoPi / dotCount;

        for (int i = 0; i < dotCount; i++) {
            float angle = _spinnerRotation + (i * angleStep);
            float x = centerX + (float)Math.Cos(angle) * radius;
            float y = centerY + (float)Math.Sin(angle) * radius;

            // Fade dots based on position
            float fade = (i / (float)dotCount);
            Color dotColor = Color.White * fade;

            int dotSize = 8;
            _spriteBatch.Draw(_pixelTexture,
                new Rectangle((int)x - dotSize / 2, (int)y - dotSize / 2, dotSize, dotSize),
                dotColor);
        }
    }

    private void DrawRectangleBorder(int x, int y, int width, int height, int thickness, Color color) {
        // Top
        _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, width, thickness), color);
        // Bottom
        _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + height - thickness, width, thickness), color);
        // Left
        _spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, thickness, height), color);
        // Right
        _spriteBatch.Draw(_pixelTexture, new Rectangle(x + width - thickness, y, thickness, height), color);
    }
}