using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chip8Emu.Displays
{
    public abstract class Display
    {
        protected int _xPos, _yPos, _width, _height;

        public Display(int xPos, int yPos, int width, int height) 
        {
            _xPos = xPos;
            _yPos = yPos;
            _width = width;
            _height = height;
        }

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    }
}
