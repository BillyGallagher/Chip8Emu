using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        readonly byte[] V = new byte[16];
        ushort I, PC = 0x200;

        readonly byte[] RAM = new byte[0x1000];
        readonly List<ushort> Stack = new List<ushort>();

        readonly Dictionary<byte, Action<OpCode>> OpCodes;
        readonly Dictionary<byte, Action<OpCode>> MiscOpCodes;

        byte DelayTimer;
        readonly Random Rng = new Random();

        readonly bool[,] DisplayBuffer = new bool[64, 32];

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private readonly Stopwatch Stopwatch500Hz = Stopwatch.StartNew();
        private readonly TimeSpan ElapsedTimeTarget500Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 500);
        private readonly Stopwatch Stopwatch60Hz = Stopwatch.StartNew();
        private readonly TimeSpan ElapsedTimeTarget60Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

        public Chip8()
        {
            _graphics = new GraphicsDeviceManager(this);

            OpCodes = new Dictionary<byte, Action<OpCode>>()
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

            MiscOpCodes = new Dictionary<byte, Action<OpCode>>()
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
            LoadProgram(File.ReadAllBytes(@"C:\dev\Chip8Emu\test_opcode.ch8"));
        }

        protected override void Initialize()
        {
            // TODO: Figure out dimensions
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Stopwatch500Hz.Elapsed >= ElapsedTimeTarget500Hz)
            {
                var rawOpCode = (ushort)(RAM[PC++] << 8 | RAM[PC++]);

                var opCode = new OpCode
                {
                    FullOpCode = rawOpCode,
                    NNN = (ushort)(rawOpCode & 0x0fff),
                    NN = (byte)(rawOpCode & 0x00ff),
                    N = (byte)(rawOpCode & 0x000f),
                    X = (byte)((rawOpCode & 0x0f00) >> 8),
                    Y = (byte)((rawOpCode & 0x00f0) >> 4)
                };

                Debug.WriteLine(rawOpCode);

                var msb = (byte)(rawOpCode >> 12);
                OpCodes[msb](opCode);
            }
            if (Stopwatch60Hz.Elapsed >= ElapsedTimeTarget60Hz)
            {

            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var onPixel = new Texture2D(_graphics.GraphicsDevice, 5, 5);
            Color[] onData = new Color[5 * 5];
            for (int i = 0; i < onData.Length; i++) { onData[i] = Color.White; }

            var offPixel = new Texture2D(_graphics.GraphicsDevice, 5, 5);
            Color[] offData = new Color[5 * 5];
            for (int i = 0; i < offData.Length; i++) { offData[i] = Color.White; }

            _spriteBatch.Begin();
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    bool isOn = DisplayBuffer[x, y];
                    Vector2 coordinate = new Vector2(x * 5, y * 5);
                    _spriteBatch.Draw(isOn ? onPixel : offPixel, coordinate, Color.White);
                }
            }
            _spriteBatch.End();
        }


        public void LoadProgram(byte[] rom)
        {
            Array.Copy(rom, 0, RAM, 0x200, rom.Length);
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
                RAM[address + i] = sprite[i];
            }
        }

        // ---- OpCodes ----
        void ClearOrReturn(OpCode opCode) 
        {
            if (opCode.N == 0x0) // Clear display
            {
                _graphics.GraphicsDevice.Clear(Color.Black);
            }
            else if (opCode.N == 0xe) // Return
            {
                PC = Stack.Last();
                Stack.RemoveAt(Stack.Count - 1);
            }
        }

        void Jump(OpCode opCode) 
        {
            PC = opCode.NNN;
        }

        void Call(OpCode opCode) 
        {
            Stack.Add(PC);
            PC = opCode.NNN;
        }

        void SkipIfXEqual(OpCode opCode) 
        { 
            if (V[opCode.X] == opCode.NN)
            {
                PC += 2;
            }
        }

        void SkipIfXNotEqual(OpCode opCode) 
        { 
            if (V[opCode.X] != opCode.NN)
            {
                PC += 2;
            }
        }

        void SkipIfXEqualToY(OpCode opCode) 
        { 
            if (V[opCode.X] == V[opCode.Y])
            {
                PC += 2;
            }
        }

        void SetX(OpCode opCode) 
        {
            V[opCode.X] = opCode.NN;
        }

        void IncreaseX(OpCode opCode) 
        {
            V[opCode.X] += opCode.NN;       
        }

        void Arithmetic(OpCode opCode) 
        {
            switch (opCode.N)
            {
                case 0x0:
                    V[opCode.X] = V[opCode.Y];
                    break;
                case 0x1:
                    V[opCode.X] = (byte)(V[opCode.X] | V[opCode.Y]);
                    break;
                case 0x2:
                    V[opCode.X] = (byte)(V[opCode.X] & V[opCode.Y]);
                    break;
                case 0x3:
                    V[opCode.X] = (byte)(V[opCode.X] ^ V[opCode.Y]);
                    break;
                case 0x4:
                    var sum = (byte)(V[opCode.X] + V[opCode.Y]);
                    V[0xf] = (byte)(sum > 0xff ? 1 : 0);
                    V[opCode.X] = sum;
                    break;
                case 0x5:
                    V[0xf] = (byte)(V[opCode.Y] > V[opCode.X] ? 1 : 0);
                    V[opCode.X] = (byte)(V[opCode.X] - V[opCode.Y]);
                    break;
                case 0x6:
                    V[0xf] = (byte)(V[opCode.X] & 0x1);
                    V[opCode.X] = (byte)(V[opCode.X] >> 1);
                    break;
                case 0x7:
                    V[0xf] = (byte)(V[opCode.X] > V[opCode.Y] ? 1 : 0);
                    V[opCode.X] = (byte)(V[opCode.Y] - V[opCode.X]);
                    break;
                case 0xe:
                    V[0xf] = (byte)(V[opCode.X] >> 3);
                    V[opCode.X] = (byte)(V[opCode.X] << 1);
                    break;
                default:
                    break;
            }
        }

        void SkipIfXNotEqualToY(OpCode opCode) 
        { 
            if (V[opCode.X] != V[opCode.Y])
            {
                PC += 2;
            }
        }

        void SetI(OpCode opCode) 
        {
            I = opCode.NNN;
        }

        void JumpWithOffset(OpCode opCode) 
        {
            PC = (ushort)(V[0] + opCode.NNN);       
        }

        void Random(OpCode opCode) 
        {
            V[opCode.X] = (byte)(Rng.Next(0, 256) & opCode.NN);
        }

        void Display(OpCode opCode) 
        {
            V[0xF] = 0;
            for (var line = 0; line < opCode.N; line++)
            {
                var spriteForLine = RAM[I + line];
                
                for (var bit = 0; bit < 8; bit++)
                {
                    var x = V[opCode.X] + bit;
                    var y = V[opCode.Y] + line;

                    bool oldBit = DisplayBuffer[x, y];
                    bool newBit = ((spriteForLine >> (7 - bit)) & 1) != 0;

                    DisplayBuffer[x, y] = oldBit ^ newBit;

                    if (oldBit == true && newBit == true) // Collision
                    {
                        V[0xF] = 1;
                    }
                }
            }

            //Draw(DisplayBuffer);
        }

        void SkipOnKey(OpCode opCode) { /* TODO: Implement */ }

        void MiscOperations(OpCode opCode) 
        { 
            if (MiscOpCodes.ContainsKey(opCode.NN))
            {
                MiscOpCodes[opCode.NN](opCode);
            }
        }

        void SetXToDelayTimer(OpCode opCode) 
        {
            V[opCode.X] = DelayTimer; 
        }

        void ReadKey(OpCode opCode) { /* TODO: Implement */ }

        void SetDelayTimer(OpCode opCode) 
        {
            DelayTimer = V[opCode.X];       
        }

        void SetSoundTimer(OpCode opCode) { /* TODO: Implement */ }

        void AddXToI(OpCode opCode) 
        {
            I += V[opCode.X];       
        }

        void SetIToCharacter(OpCode opCode) 
        {
            I = RAM[5 * opCode.X];
        }

        void BinaryCodedDecimal(OpCode opCode) 
        {
            var value = (int)(V[opCode.X]);

            // Hundreds
            RAM[I] = (byte)(((value % 1000) - (value % 100)) / 100);

            // Tens
            RAM[I] = (byte)(((value % 100) - (value % 10)) / 10);

            // Ones
            RAM[I] = (byte)(value % 10);
        }

        void DumpRegisters(OpCode opCode) 
        {
            for (byte index = 0; index < opCode.X; index++)
            {
                RAM[I + index] = V[index];
            }
        }

        void LoadRegisters(OpCode opCode) 
        { 
            for (byte index = 0; index < opCode.X; index++)
            {
                V[index] = RAM[I + index];
            }
        }
    }
}
