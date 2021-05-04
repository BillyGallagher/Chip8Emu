using Chip8Emu.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Chip8Emu.Components
{
    public class Processor
    {
        // Memory and registers
        private ushort _pCounter = 0x200;
        private readonly Memory _memory;
        private bool[,] _displayBuffer;

        // OpCodes
        private Dictionary<byte, Action<OpCode>> _opCodes;
        private Dictionary<byte, Action<OpCode>> _miscOpCodes;
        public OpCode CurrentOpCode { get; private set; }

        // Blocking OpCode (wait for input)
        private bool _waitingForInput = false;
        private OpCode _blockingOpCode;

        // Timers
        private byte _delayTimer;
        private readonly Stopwatch _stopwatch500Hz = Stopwatch.StartNew();
        private readonly TimeSpan _elapsedTimeTarget500Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 500);
        private readonly Stopwatch _stopwatch60Hz = Stopwatch.StartNew();
        private readonly TimeSpan _elapsedTimeTarget60Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);


        private readonly Random _rng = new Random();


        public byte CurrentKeyValue { get;  set; } = 0x00;

        public Processor(Memory memory, bool[,] displayBuffer) 
        {
            _memory = memory;
            _displayBuffer = displayBuffer;
            RegisterOpCodes();
        }

        public void Update()
        {
            if (_stopwatch500Hz.Elapsed >= _elapsedTimeTarget500Hz)
            {
                if (_waitingForInput && CurrentKeyValue != 0x00)
                {
                    _memory.Registers[_blockingOpCode.X] = CurrentKeyValue;
                    _waitingForInput = false;
                    _blockingOpCode = null;
                }
                else
                {
                    var rawOpCode = (ushort)(_memory.RAM[_pCounter++] << 8 | _memory.RAM[_pCounter++]);

                    var opCode = new OpCode
                    {
                        FullOpCode = rawOpCode,
                        NNN = (ushort)(rawOpCode & 0x0fff),
                        NN = (byte)(rawOpCode & 0x00ff),
                        N = (byte)(rawOpCode & 0x000f),
                        X = (byte)((rawOpCode & 0x0f00) >> 8),
                        Y = (byte)((rawOpCode & 0x00f0) >> 4)
                    };
                    CurrentOpCode = opCode;

                    var msb = (byte)(rawOpCode >> 12);
                    _opCodes[msb](opCode);
                }
            }
            if (_stopwatch60Hz.Elapsed > _elapsedTimeTarget60Hz)
            {
                _delayTimer--;
                _stopwatch60Hz.Restart();
            }
        }

        #region Helpers
        private void RegisterOpCodes()
        {
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
        }
        #endregion

        #region OpCodes
        private void ClearOrReturn(OpCode opCode) 
        {
            if (opCode.N == 0x0) // Clear display
            {
                _displayBuffer = new bool[64, 32];
            }
            else if (opCode.N == 0xe) // Return
            {
                _pCounter = _memory.Stack.Last();
                _memory.Stack.RemoveAt(_memory.Stack.Count - 1);
            }
        }

        private void Jump(OpCode opCode) 
        {
            _pCounter = opCode.NNN;
        }

        private void Call(OpCode opCode) 
        {
            _memory.Stack.Add(_pCounter);
            _pCounter = opCode.NNN;
        }

        private void SkipIfXEqual(OpCode opCode) 
        { 
            if (_memory.Registers[opCode.X] == opCode.NN)
            {
                _pCounter += 2;
            }
        }

        private void SkipIfXNotEqual(OpCode opCode) 
        { 
            if (_memory.Registers[opCode.X] != opCode.NN)
            {
                _pCounter += 2;
            }
        }

        private void SkipIfXEqualToY(OpCode opCode) 
        { 
            if (_memory.Registers[opCode.X] == _memory.Registers[opCode.Y])
            {
                _pCounter += 2;
            }
        }

        private void SetX(OpCode opCode) 
        {
            _memory.Registers[opCode.X] = opCode.NN;
        }

        private void IncreaseX(OpCode opCode) 
        {
            _memory.Registers[opCode.X] += opCode.NN;       
        }

        private void Arithmetic(OpCode opCode) 
        {
            switch (opCode.N)
            {
                case 0x0:
                    _memory.Registers[opCode.X] = _memory.Registers[opCode.Y];
                    break;
                case 0x1:
                    _memory.Registers[opCode.X] = (byte)(_memory.Registers[opCode.X] | _memory.Registers[opCode.Y]);
                    break;
                case 0x2:
                    _memory.Registers[opCode.X] = (byte)(_memory.Registers[opCode.X] & _memory.Registers[opCode.Y]);
                    break;
                case 0x3:
                    _memory.Registers[opCode.X] = (byte)(_memory.Registers[opCode.X] ^ _memory.Registers[opCode.Y]);
                    break;
                case 0x4:
                    var sum = (byte)(_memory.Registers[opCode.X] + _memory.Registers[opCode.Y]);
                    _memory.Registers[0xf] = (byte)(sum > 0xff ? 1 : 0);
                    _memory.Registers[opCode.X] = sum;
                    break;
                case 0x5:
                    _memory.Registers[0xf] = (byte)(_memory.Registers[opCode.Y] > _memory.Registers[opCode.X] ? 1 : 0);
                    _memory.Registers[opCode.X] = (byte)(_memory.Registers[opCode.X] - _memory.Registers[opCode.Y]);
                    break;
                case 0x6:
                    _memory.Registers[0xf] = (byte)(_memory.Registers[opCode.X] & 0x1);
                    _memory.Registers[opCode.X] = (byte)(_memory.Registers[opCode.X] >> 1);
                    break;
                case 0x7:
                    _memory.Registers[0xf] = (byte)(_memory.Registers[opCode.X] > _memory.Registers[opCode.Y] ? 1 : 0);
                    _memory.Registers[opCode.X] = (byte)(_memory.Registers[opCode.Y] - _memory.Registers[opCode.X]);
                    break;
                case 0xe:
                    _memory.Registers[0xf] = (byte)(_memory.Registers[opCode.X] >> 3);
                    _memory.Registers[opCode.X] = (byte)(_memory.Registers[opCode.X] << 1);
                    break;
                default:
                    break;
            }
        }

        private void SkipIfXNotEqualToY(OpCode opCode) 
        { 
            if (_memory.Registers[opCode.X] != _memory.Registers[opCode.Y])
            {
                _pCounter += 2;
            }
        }

        private void SetI(OpCode opCode) 
        {
            _memory.AddressRegister = opCode.NNN;
        }

        private void JumpWithOffset(OpCode opCode) 
        {
            _pCounter = (ushort)(_memory.Registers[0] + opCode.NNN);       
        }

        private void Random(OpCode opCode) 
        {
            _memory.Registers[opCode.X] = (byte)(_rng.Next(0, 256) & opCode.NN);
        }

        private void Display(OpCode opCode) 
        {
            _memory.Registers[0xF] = 0;
            for (var line = 0; line < opCode.N; line++)
            {
                var spriteForLine = _memory.RAM[_memory.AddressRegister + line];
                
                for (var bit = 0; bit < 8; bit++)
                {
                    var x = (_memory.Registers[opCode.X] + bit) % 64;
                    var y = (_memory.Registers[opCode.Y] + line) % 32;

                    bool oldBit = _displayBuffer[x, y];
                    bool newBit = ((spriteForLine >> (7 - bit)) & 1) != 0;

                    _displayBuffer[x, y] = oldBit ^ newBit;

                    if (oldBit == true && newBit == true) // Collision
                    {
                        _memory.Registers[0xF] = 1;
                    }
                }
            }
        }

        private void SkipOnKey(OpCode opCode) 
        {
            var keyValue = _memory.Registers[opCode.X];

            if ((opCode.NN == 0x9E && keyValue == CurrentKeyValue) // if (key == Vx)
                || (opCode.NN == 0xA1 && keyValue != CurrentKeyValue)) // if (key != Vx)
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
            _memory.Registers[opCode.X] = _delayTimer; 
        }

        private void ReadKey(OpCode opCode) 
        {
            _waitingForInput = true;
            _blockingOpCode = opCode;
        }

        private void SetDelayTimer(OpCode opCode) 
        {
            _delayTimer = _memory.Registers[opCode.X];       
        }

        private void SetSoundTimer(OpCode opCode) { /* TODO: Implement */ }

        private void AddXToI(OpCode opCode) 
        {
            _memory.AddressRegister += _memory.Registers[opCode.X];       
        }

        private void SetIToCharacter(OpCode opCode) 
        {
            _memory.AddressRegister = _memory.RAM[5 * opCode.X];
        }

        private void BinaryCodedDecimal(OpCode opCode) 
        {
            var value = (int)(_memory.Registers[opCode.X]);

            // Hundreds
            _memory.AddressRegister = (byte)(((value % 1000) - (value % 100)) / 100);

            // Tens
            _memory.AddressRegister = (byte)(((value % 100) - (value % 10)) / 10);

            // Ones
            _memory.AddressRegister = (byte)(value % 10);
        }

        private void DumpRegisters(OpCode opCode) 
        {
            for (byte index = 0; index < opCode.X; index++)
            {
                _memory.RAM[_memory.AddressRegister + index] = _memory.Registers[index];
            }
        }

        private void LoadRegisters(OpCode opCode) 
        { 
            for (byte index = 0; index < opCode.X; index++)
            {
                _memory.Registers[index] = _memory.RAM[_memory.AddressRegister + index];
            }
        }
        #endregion
    }
}
