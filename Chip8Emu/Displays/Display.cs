using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chip8Emu.Displays
{
    public abstract class Display
    {
        protected Vector2 _position, _size;
        protected readonly GraphicsDevice _graphicsDevice;

        public Display(Vector2 position, Vector2 size, GraphicsDevice graphicsDevice) 
        {
            _position = position;
            _size = size;
            _graphicsDevice = graphicsDevice;
        }

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    }
}
