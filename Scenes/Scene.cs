using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

#nullable enable
namespace MineGameB.Scenes;
public class Scene(string? name = null) {
    public string Name { get; private set; } = name ?? "unnamed";

#nullable disable
    public static List<Scene> Scenes { get; protected set; } = [];
    protected static string SelectedScene { get; set; } = "game";

    public static void AddScene(Scene sc) {
        sc.Load();
        Scenes.Add(sc);
    }

    public static Scene GetScene(string name) {
        foreach (Scene scene in Scenes)
            if (scene.Name == name)
                return scene;
        return new Scene(name);
    }

    public static Scene GetCurentScene() => GetScene(SelectedScene);
    public virtual void Load() { }

    public virtual void Update(GameTime gameTime) { }

    public virtual void Draw(SpriteBatch spriteBatch) { }
}
