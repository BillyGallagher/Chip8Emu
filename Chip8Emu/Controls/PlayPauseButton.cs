using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Chip8Emu.Controls
{
    public class PlayPauseButton : Control
    {
        private Texture2D _playTexture, _pauseTexture;
        private Color _playColor = Color.Green, _pauseColor = Color.Red;
        private const double _msDelay = 200;
        private double _msSinceClick = 0;
        private bool _paused { get; set; }

        public Action OnClick { get; set; } = null;

        public PlayPauseButton(Vector2 position, Vector2 size) : base(position, size)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(_paused ? _playTexture : _pauseTexture,
                _position,
                Color.White);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice)
        {
            _playTexture = new Texture2D(graphicsDevice, (int)_size.X, (int)_size.Y);
            Color[] textureData = new Color[(int)_size.X * (int)_size.Y];
            for (int i = 0; i < textureData.Length; i++) { textureData[i] = _playColor; };
            _playTexture.SetData(textureData);

            _pauseTexture = new Texture2D(graphicsDevice, (int)_size.X, (int)_size.Y);
            textureData = new Color[(int)_size.X * (int)_size.Y];
            for (int i = 0; i < textureData.Length; i++) { textureData[i] = _pauseColor; };
            _pauseTexture.SetData(textureData);
        }

        public override void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState)
        {
            _msSinceClick += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_msSinceClick < _msDelay)
                return;

            if (mouseState.LeftButton == ButtonState.Pressed
             && mouseState.Position.X >= _position.X
             && mouseState.Position.X <= _position.X + _size.X
             && mouseState.Position.Y >= _position.Y
             && mouseState.Position.Y <= _position.Y + _size.Y)
            {
                OnClick();
                TogglePause();
                _msSinceClick = 0;
            }
        }

        private void TogglePause()
        {
            _paused = !_paused;
        }
    }
}
