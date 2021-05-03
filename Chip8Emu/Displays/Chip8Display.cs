using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chip8Emu.Displays
{
    public class Chip8Display : Display
    {
        private readonly bool[,] _displayBuffer;
        private Texture2D _onPixel;
        private Texture2D _offPixel;

        public Chip8Display(
            bool[,] displayBuffer,
            Vector2 position,
            Vector2 size) : base(position, size) 

        {
            _displayBuffer = displayBuffer;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime) 
        {
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    bool isOn = _displayBuffer[x, y];
                    spriteBatch.Draw(isOn ? _onPixel : _offPixel, new Rectangle(x * 10, y * 10, 10, 10), Color.White);
                }
            }
        }

        public override void LoadContent(GraphicsDevice graphicsDevice)
        {
            _onPixel = new Texture2D(graphicsDevice, 1, 1);
            _onPixel.SetData(new[] { Color.White });

            _offPixel = new Texture2D(graphicsDevice, 1, 1);
            _offPixel.SetData(new[] { Color.Black });
        }
    }
}
