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
            GraphicsDevice graphicsDevice,
            int xPos,
            int yPos,
            int width,
            int height) : base(xPos, yPos, width, height) 
        {
            _displayBuffer = displayBuffer;
            CreatePixels(graphicsDevice);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime) 
        {
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    bool isOn = _displayBuffer[x, y];
                    spriteBatch.Draw(isOn ? _onPixel : _offPixel, new Rectangle(x * 5, y * 5, 5, 5), Color.White);
                }
            }
        }

        private void CreatePixels(GraphicsDevice graphicsDevice)
        {
            _onPixel = new Texture2D(graphicsDevice, 5, 5);
            Color[] onData = new Color[5 * 5];
            for (int i = 0; i < onData.Length; i++) { onData[i] = Color.White; }
            _onPixel.SetData(onData);

            _offPixel = new Texture2D(graphicsDevice, 5, 5);
            Color[] offData = new Color[5 * 5];
            for (int i = 0; i < offData.Length; i++) { offData[i] = Color.Black; }
            _offPixel.SetData(offData);

        }
    }
}
