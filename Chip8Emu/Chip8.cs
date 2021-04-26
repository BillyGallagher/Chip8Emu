using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Chip8Emu
{
    public class Chip8 : Game
    {
        // Memory and registers
        private readonly byte[] _registers = new byte[16];
        private ushort _addressRegister;
        private ushort _pCounter = 0x200;
        private readonly byte[] _memory = new byte[0x1000];
        private readonly List<ushort> _stack = new List<ushort>();

        // OpCodes
        private readonly Dictionary<byte, Action<OpCode>> _opCodes;
        private readonly Dictionary<byte, Action<OpCode>> _miscOpCodes;

        // Graphics
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _onPixel;
        private Texture2D _offPixel;
        private bool[,] _displayBuffer = new bool[64, 32];
        private Vector2 _baseScreenSize = new Vector2(64 * 5, 32 * 5);
        private Matrix _globalTransform;

        // Timers
        private byte _delayTimer;
        private readonly Stopwatch _stopwatch500Hz = Stopwatch.StartNew();
        private readonly TimeSpan _elapsedTimeTarget500Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 500);
        private readonly Stopwatch _stopwatch60Hz = Stopwatch.StartNew();
        private readonly TimeSpan _elapsedTimeTarget60Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

        // User input
        private Dictionary<Keys, byte> _keyToValue;
        private byte _currentKeyValue = 0x00;


        readonly Random _rng = new Random();


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

            _opCodes = new Dictionary<byte, Action<OpCode>>()
            {
                { 0x0, ClearOrReturn },
                { 0x1, Jump },
                { 0x2, Call },
                { 0x3, SkipIfXEqual },
                { 0x4, SkipIfXNotEqual },
                { 0x5, SkipIfXEqualToY },
                { 0x6, SetX },
                { 0x7, IncreaseX },
                { 0x8, Arithmetic },
                { 0x9, SkipIfXNotEqualToY },
                { 0xa, SetI },
                { 0xb, JumpWithOffset },
                { 0xc, Random },
                { 0xd, Display },
                { 0xe, SkipOnKey },
                { 0xf, MiscOperations }
            };

            _miscOpCodes = new Dictionary<byte, Action<OpCode>>()
            {
                { 0x07, SetXToDelayTimer },
                { 0x0a, ReadKey },
                { 0x15, SetDelayTimer },
                { 0x18, SetSoundTimer },
                { 0x1e, AddXToI },
                { 0x29, SetIToCharacter },
                { 0x33, BinaryCodedDecimal },
                { 0x55, DumpRegisters },
                { 0x65, LoadRegisters }
            };

            InitializeFont();
            LoadProgram(File.ReadAllBytes(@"C:\dev\Chip8Emu\roms\pong.ch8"));
        }

        protected override void Initialize()
        {
            // TODO: Figure out dimensions
            ScaleDisplayArea();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _onPixel = new Texture2D(_graphics.GraphicsDevice, 5, 5);
            Color[] onData = new Color[5 * 5];
            for (int i = 0; i < onData.Length; i++) { onData[i] = Color.White; }
            _onPixel.SetData(onData);

            _offPixel = new Texture2D(_graphics.GraphicsDevice, 5, 5);
            Color[] offData = new Color[5 * 5];
            for (int i = 0; i < offData.Length; i++) { offData[i] = Color.Black; }
            _offPixel.SetData(offData);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (_stopwatch500Hz.Elapsed >= _elapsedTimeTarget500Hz)
            {
                HandleInput();
                var rawOpCode = (ushort)(_memory[_pCounter++] << 8 | _memory[_pCounter++]);

                var opCode = new OpCode
                {
                    FullOpCode = rawOpCode,
                    NNN = (ushort)(rawOpCode & 0x0fff),
                    NN = (byte)(rawOpCode & 0x00ff),
                    N = (byte)(rawOpCode & 0x000f),
                    X = (byte)((rawOpCode & 0x0f00) >> 8),
                    Y = (byte)((rawOpCode & 0x00f0) >> 4)
                };

                var msb = (byte)(rawOpCode >> 12);
                _opCodes[msb](opCode);
            }
            if (_stopwatch60Hz.Elapsed >= _elapsedTimeTarget60Hz)
            {
                // TODO: Sound, I think
            }

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            var pressedKey = keyboardState.GetPressedKeys().Where(k => _keyToValue.Keys.Contains(k)).FirstOrDefault();

            if (pressedKey != Keys.None)
            {
                _currentKeyValue = _keyToValue[pressedKey];
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, _globalTransform);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    bool isOn = _displayBuffer[x, y];
                    _spriteBatch.Draw(isOn ? _onPixel : _offPixel, new Rectangle(x * 5, y * 5, 5, 5), Color.White);
                }
            }
            _spriteBatch.End();
        }

        public void LoadProgram(byte[] rom)
        {
            Array.Copy(rom, 0, _memory, 0x200, rom.Length);
        }

        private void InitializeFont()
        {
            var offset = 0x0;
            StoreSprite(5 * offset++, FontSprites.Zero);
            StoreSprite(5 * offset++, FontSprites.One);
            StoreSprite(5 * offset++, FontSprites.Two);
            StoreSprite(5 * offset++, FontSprites.Three);
            StoreSprite(5 * offset++, FontSprites.Four);
            StoreSprite(5 * offset++, FontSprites.Five);
            StoreSprite(5 * offset++, FontSprites.Six);
            StoreSprite(5 * offset++, FontSprites.Seven);
            StoreSprite(5 * offset++, FontSprites.Eight);
            StoreSprite(5 * offset++, FontSprites.Nine);
            StoreSprite(5 * offset++, FontSprites.A);
            StoreSprite(5 * offset++, FontSprites.B);
            StoreSprite(5 * offset++, FontSprites.C);
            StoreSprite(5 * offset++, FontSprites.D);
            StoreSprite(5 * offset++, FontSprites.E);
            StoreSprite(5 * offset++, FontSprites.F);
        }

        private void StoreSprite(int address, byte[] sprite)
        {
            for (var i = 0; i < sprite.Length; i++)
            {
                _memory[address + i] = sprite[i];
            }
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

        #region OpCodes
        private void ClearOrReturn(OpCode opCode) 
        {
            if (opCode.N == 0x0) // Clear display
            {
                _displayBuffer = new bool[64, 32];
            }
            else if (opCode.N == 0xe) // Return
            {
                _pCounter = _stack.Last();
                _stack.RemoveAt(_stack.Count - 1);
            }
        }

        private void Jump(OpCode opCode) 
        {
            _pCounter = opCode.NNN;
        }

        private void Call(OpCode opCode) 
        {
            _stack.Add(_pCounter);
            _pCounter = opCode.NNN;
        }

        private void SkipIfXEqual(OpCode opCode) 
        { 
            if (_registers[opCode.X] == opCode.NN)
            {
                _pCounter += 2;
            }
        }

        private void SkipIfXNotEqual(OpCode opCode) 
        { 
            if (_registers[opCode.X] != opCode.NN)
            {
                _pCounter += 2;
            }
        }

        private void SkipIfXEqualToY(OpCode opCode) 
        { 
            if (_registers[opCode.X] == _registers[opCode.Y])
            {
                _pCounter += 2;
            }
        }

        private void SetX(OpCode opCode) 
        {
            _registers[opCode.X] = opCode.NN;
        }

        private void IncreaseX(OpCode opCode) 
        {
            _registers[opCode.X] += opCode.NN;       
        }

        private void Arithmetic(OpCode opCode) 
        {
            switch (opCode.N)
            {
                case 0x0:
                    _registers[opCode.X] = _registers[opCode.Y];
                    break;
                case 0x1:
                    _registers[opCode.X] = (byte)(_registers[opCode.X] | _registers[opCode.Y]);
                    break;
                case 0x2:
                    _registers[opCode.X] = (byte)(_registers[opCode.X] & _registers[opCode.Y]);
                    break;
                case 0x3:
                    _registers[opCode.X] = (byte)(_registers[opCode.X] ^ _registers[opCode.Y]);
                    break;
                case 0x4:
                    var sum = (byte)(_registers[opCode.X] + _registers[opCode.Y]);
                    _registers[0xf] = (byte)(sum > 0xff ? 1 : 0);
                    _registers[opCode.X] = sum;
                    break;
                case 0x5:
                    _registers[0xf] = (byte)(_registers[opCode.Y] > _registers[opCode.X] ? 1 : 0);
                    _registers[opCode.X] = (byte)(_registers[opCode.X] - _registers[opCode.Y]);
                    break;
                case 0x6:
                    _registers[0xf] = (byte)(_registers[opCode.X] & 0x1);
                    _registers[opCode.X] = (byte)(_registers[opCode.X] >> 1);
                    break;
                case 0x7:
                    _registers[0xf] = (byte)(_registers[opCode.X] > _registers[opCode.Y] ? 1 : 0);
                    _registers[opCode.X] = (byte)(_registers[opCode.Y] - _registers[opCode.X]);
                    break;
                case 0xe:
                    _registers[0xf] = (byte)(_registers[opCode.X] >> 3);
                    _registers[opCode.X] = (byte)(_registers[opCode.X] << 1);
                    break;
                default:
                    break;
            }
        }

        private void SkipIfXNotEqualToY(OpCode opCode) 
        { 
            if (_registers[opCode.X] != _registers[opCode.Y])
            {
                _pCounter += 2;
            }
        }

        private void SetI(OpCode opCode) 
        {
            _addressRegister = opCode.NNN;
        }

        private void JumpWithOffset(OpCode opCode) 
        {
            _pCounter = (ushort)(_registers[0] + opCode.NNN);       
        }

        private void Random(OpCode opCode) 
        {
            _registers[opCode.X] = (byte)(_rng.Next(0, 256) & opCode.NN);
        }

        private void Display(OpCode opCode) 
        {
            _registers[0xF] = 0;
            for (var line = 0; line < opCode.N; line++)
            {
                var spriteForLine = _memory[_addressRegister + line];
                
                for (var bit = 0; bit < 8; bit++)
                {
                    var x = _registers[opCode.X] + bit;
                    var y = _registers[opCode.Y] + line;

                    bool oldBit = _displayBuffer[x, y];
                    bool newBit = ((spriteForLine >> (7 - bit)) & 1) != 0;

                    _displayBuffer[x, y] = oldBit ^ newBit;

                    if (oldBit == true && newBit == true) // Collision
                    {
                        _registers[0xF] = 1;
                    }
                }
            }
        }

        private void SkipOnKey(OpCode opCode) 
        {
            var keyValue = _registers[opCode.X];

            if ((opCode.NN == 0x9E && keyValue == _currentKeyValue) // if (key == Vx)
                || (opCode.NN == 0xA1 && keyValue != _currentKeyValue)) // if (key != Vx)
            {
                _pCounter += 2;
            }
        }

        private void MiscOperations(OpCode opCode) 
        { 
            if (_miscOpCodes.ContainsKey(opCode.NN))
            {
                _miscOpCodes[opCode.NN](opCode);
            }
        }

        private void SetXToDelayTimer(OpCode opCode) 
        {
            _registers[opCode.X] = _delayTimer; 
        }

        private void ReadKey(OpCode opCode) { /* TODO: Implement */ }

        private void SetDelayTimer(OpCode opCode) 
        {
            _delayTimer = _registers[opCode.X];       
        }

        private void SetSoundTimer(OpCode opCode) { /* TODO: Implement */ }

        private void AddXToI(OpCode opCode) 
        {
            _addressRegister += _registers[opCode.X];       
        }

        private void SetIToCharacter(OpCode opCode) 
        {
            _addressRegister = _memory[5 * opCode.X];
        }

        private void BinaryCodedDecimal(OpCode opCode) 
        {
            var value = (int)(_registers[opCode.X]);

            // Hundreds
            _memory[_addressRegister] = (byte)(((value % 1000) - (value % 100)) / 100);

            // Tens
            _memory[_addressRegister] = (byte)(((value % 100) - (value % 10)) / 10);

            // Ones
            _memory[_addressRegister] = (byte)(value % 10);
        }

        private void DumpRegisters(OpCode opCode) 
        {
            for (byte index = 0; index < opCode.X; index++)
            {
                _memory[_addressRegister + index] = _registers[index];
            }
        }

        private void LoadRegisters(OpCode opCode) 
        { 
            for (byte index = 0; index < opCode.X; index++)
            {
                _registers[index] = _memory[_addressRegister + index];
            }
        }
        #endregion
    }
}
