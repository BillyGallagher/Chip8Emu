using Chip8Emu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip8Emu.Displays
{
    public class RegisterDisplay : Display
    {
        private readonly Memory _memory;
        private Texture2D _texture;

        public RegisterDisplay(Memory memory, Vector2 position, Vector2 size, GraphicsDevice graphicsDevice) : base(position, size, graphicsDevice)
        {
            _memory = memory;
            InitializeTexture();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(_texture, _position, Color.White);
        }

        private void InitializeTexture()
        {
            _texture = new Texture2D(_graphicsDevice, (int)_size.X, (int)_size.Y);
            Color[] textureData = new Color[(int)_size.X * (int)_size.Y];
            for (int i = 0; i < textureData.Length; i++) { textureData[i] = Color.PowderBlue; };
            _texture.SetData(textureData);
        }
    }
}
