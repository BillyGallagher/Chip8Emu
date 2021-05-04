using Chip8Emu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chip8Emu.Displays
{
    public class InstructionDisplay : Display
    {
        private readonly Processor _processor;
        private readonly ContentManager _contentManager;

        private Texture2D _background;
        private Color _backgroundColor = Color.Black;
        private Texture2D _border;
        private Color _borderColor = Color.SlateGray;

        private SpriteFont _font;
        private Color _fontColor = Color.White;

        public InstructionDisplay(Processor processor, ContentManager contentManager, Vector2 position, Vector2 size) : base(position, size) 
        {
            _processor = processor;
            _contentManager = contentManager;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(_background, _position, Color.White);
            spriteBatch.Draw(_border, new Rectangle((int)_position.X, (int)_position.Y, 5, (int)_size.Y), Color.White);

            spriteBatch.DrawString(_font, $"Current Instruction: 0x{_processor.CurrentOpCode.FullOpCode:X2}", new Vector2(_position.X + 5, _position.Y + 5), _fontColor);
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
        }
    }
}
