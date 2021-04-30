using System;
using System.Collections.Generic;
using System.Text;

namespace Chip8Emu.Models
{
    public class OpCode
    {
        public ushort FullOpCode;
        public ushort NNN { get; set; }
        public byte NN { get; set; }
        public byte N { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
    }
}
