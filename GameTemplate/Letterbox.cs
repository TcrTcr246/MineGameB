using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameTemplate;

public static class Letterbox {
    public static int ViewportWidth { get; private set; }
    public static int ViewportHeight { get; private set; }
    public static int VirtualWidth { get; private set; }
    public static int VirtualHeight { get; private set; }
    public static int ScreenWidth { get; private set; }
    public static int ScreenHeight { get; private set; }
    public static int OffsetX { get; private set; }
    public static int OffsetY { get; private set; }
    public static float Scale { get; private set; } = 1f;

    private static GraphicsDevice graphicsDevice;
    private static GameWindow window;

    public static void Initialize(GraphicsDevice gd, GameWindow w) {
        graphicsDevice = gd;
        window = w;
    }

    public static void UpdateScaleMatrix(int virtualWidth, int virtualHeight) {
        if (graphicsDevice == null || window == null)
            throw new System.Exception("Letterbox not initialized! Call Letterbox.Initialize first.");

        int windowWidth = window.ClientBounds.Width;
        int windowHeight = window.ClientBounds.Height;

        float scaleX = windowWidth / (float)virtualWidth;
        float scaleY = windowHeight / (float)virtualHeight;
        Scale = MathHelper.Min(scaleX, scaleY);

        int viewportWidth = (int)(virtualWidth * Scale);
        int viewportHeight = (int)(virtualHeight * Scale);

        OffsetX = (windowWidth - viewportWidth) / 2;
        OffsetY = (windowHeight - viewportHeight) / 2;

        VirtualWidth = virtualWidth;
        VirtualHeight = virtualHeight;
        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
        ScreenWidth = windowWidth;
        ScreenHeight = windowHeight;

        graphicsDevice.Viewport = new Viewport(OffsetX, OffsetY, viewportWidth, viewportHeight);
    }
}
