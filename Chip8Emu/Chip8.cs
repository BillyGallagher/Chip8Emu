using Chip8Emu.Components;
using Chip8Emu.Displays;
using Chip8Emu.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Chip8Emu
{
    public class Chip8 : Game
    {
        // Graphics
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Vector2 _baseScreenSize = new Vector2(64 * 5, 32 * 5);
        private Matrix _globalTransform;
        private bool[,] _displayBuffer = new bool[64, 32];

        // User input
        private Dictionary<Keys, byte> _keyToValue;

        private readonly List<Display> _displays = new List<Display>();
        private readonly Processor _processor;
        private readonly Memory _memory;

        public Chip8()
        {
            _graphics = new GraphicsDeviceManager(this);
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;

            _keyToValue = new Dictionary<Keys, byte>() 
            {
                { Keys.NumPad0, 0x00 },
                { Keys.NumPad1, 0x01 },
                { Keys.NumPad2, 0x02 },
                { Keys.NumPad3, 0x03 },
                { Keys.NumPad4, 0x04 },
                { Keys.NumPad5, 0x05 },
                { Keys.NumPad6, 0x06 },
                { Keys.NumPad7, 0x07 },
                { Keys.NumPad8, 0x08 },
                { Keys.NumPad9, 0x09 },
                { Keys.A, 0x0A },
                { Keys.B, 0x0B },
                { Keys.C, 0x0C },
                { Keys.D, 0x0D },
                { Keys.E, 0x0E },
                { Keys.F, 0x0F }
            };


            var displayBuffer = new bool[64, 32];


            _memory = new Memory(File.ReadAllBytes(@"C:\dev\Chip8Emu\roms\pong.ch8"));
            _processor = new Processor(_memory, _displayBuffer);
        }

        protected override void Initialize()
        {
            // TODO: Figure out dimensions
            ScaleDisplayArea();
            _displays.Add(new Chip8Display(_displayBuffer, GraphicsDevice, 0, 0, 64, 32));
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            HandleInput();
            _processor.Update();
            base.Update(gameTime);
        }

        private void HandleInput()
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            var pressedKey = keyboardState.GetPressedKeys().Where(k => _keyToValue.Keys.Contains(k)).LastOrDefault();

            if (pressedKey != Keys.None)
            {
                _processor.CurrentKeyValue = _keyToValue[pressedKey];
            }
            else
            {
                _processor.CurrentKeyValue = 0x00;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, _globalTransform);

            foreach (var display in _displays)
            {
                display.Draw(_spriteBatch, gameTime);
            }

            _spriteBatch.End();
        }

        private void OnResize(object sender, EventArgs e) 
        {
            ScaleDisplayArea();
        }

        private void ScaleDisplayArea()
        {
            float horizontalScaling = GraphicsDevice.PresentationParameters.BackBufferWidth / _baseScreenSize.X;
            float verticalScaling = GraphicsDevice.PresentationParameters.BackBufferHeight / _baseScreenSize.Y;

            Vector3 scaleFactor = new Vector3(horizontalScaling, verticalScaling, 1);
            _globalTransform = Matrix.CreateScale(scaleFactor);
        }
    }
}
