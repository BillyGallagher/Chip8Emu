using Chip8Emu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip8Emu.Displays
{
    public class RegisterDisplay : Display
    {
        private readonly Memory _memory;
        private readonly ContentManager _contentManager;

        // This should probably be an actual sprite loaded through the content manager
        private Texture2D _background;
        private Color _backgroundColor = Color.Black;
        private Texture2D _border;
        private Color _borderColor = Color.SlateGray;

        private SpriteFont _font;
        private Color _fontColor = Color.White;

        // For display columns
        private const int xOffset = 180;
        private const int yOffset = 20;


        public RegisterDisplay(Memory memory, ContentManager contentManager, Vector2 position, Vector2 size) : base(position, size)
        {
            _memory = memory;
            _contentManager = contentManager;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(_background, _position, Color.White);
            spriteBatch.Draw(_border, new Rectangle((int)_position.X, (int)_position.Y, 5, (int)_size.Y), Color.White);
            spriteBatch.Draw(_border, new Rectangle((int)_position.X, (int)_position.Y + (int)_size.Y - 5, (int)_size.X, 5), Color.White);
            DrawRegisterValues(spriteBatch);

            base.Draw(spriteBatch, gameTime);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice)
        {
            _background = new Texture2D(graphicsDevice, (int)_size.X, (int)_size.Y);
            Color[] textureData = new Color[(int)_size.X * (int)_size.Y];
            for (int i = 0; i < textureData.Length; i++) { textureData[i] = _backgroundColor; };
            _background.SetData(textureData);

            _border = new Texture2D(graphicsDevice, 1, 1);
            _border.SetData(new[] { _borderColor });

            _font = _contentManager.Load<SpriteFont>("Display");
            base.LoadContent(graphicsDevice);
        }

        private void DrawRegisterValues(SpriteBatch spriteBatch)
        {
            int i = 0;
            string text;
            Vector2 pos = new Vector2(_position.X + 10, _position.Y + 10);

            while (i < 16)
            {
                text = $"R{i + 1}: 0x{_memory.Registers[i]:X2}";

                spriteBatch.DrawString(_font, text, pos, _fontColor);

                if (i % 2 == 0) {
                    pos.X += xOffset;
                }
                else
                {
                    pos.X -= xOffset;
                    pos.Y += yOffset;
                }

                i++;
            }

            spriteBatch.DrawString(_font, $"I: 0x{_memory.AddressRegister:X2}", pos, _fontColor);
        }
    }
}
