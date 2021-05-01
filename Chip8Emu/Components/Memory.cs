using Chip8Emu.Resources;
using System;
using System.Collections.Generic;

namespace Chip8Emu.Components
{
    public class Memory
    {
        public readonly byte[] Registers = new byte[16];
        public ushort AddressRegister;
        public readonly byte[] RAM = new byte[0x1000];
        public readonly List<ushort> Stack = new List<ushort>();

        public Memory(byte[] rom) 
        {
            InitializeFont();
            LoadRom(rom);
        }

        #region
        private void LoadRom(byte[] rom)
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
        #endregion
    }
}
