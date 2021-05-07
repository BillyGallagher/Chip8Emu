using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chip8Emu.Controls
{
    public abstract class Control
    {
        protected Vector2 _position, _size;

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);
        public abstract void LoadContent(GraphicsDevice graphicsDevice);
        public abstract void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState);

        public Control(Vector2 position, Vector2 size) 
        {
            _position = position;
            _size = size;
        }
    }
}
