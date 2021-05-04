using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chip8Emu.Displays
{
    public abstract class Display
    {
        protected Vector2 _position, _size;

        public Display(Vector2 position, Vector2 size) 
        {
            _position = position;
            _size = size;
        }

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);
        public abstract void LoadContent(GraphicsDevice graphicsDevice);
    }
}
