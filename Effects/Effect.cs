using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MineGameB.World;

namespace MineGameB.Effects;
public class Effect {
    protected Microsoft.Xna.Framework.Graphics.Effect effect;

    public Effect(Microsoft.Xna.Framework.Graphics.Effect effect) {
        this.effect = effect;
    }

    public virtual void Update(GameTime gameTime) { }
    public virtual void Draw(SpriteBatch spriteBatch) { }
}
