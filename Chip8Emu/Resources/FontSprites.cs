namespace Chip8Emu.Resources
{
    // Reference: http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#2.4
    static class FontSprites
    {
        // F:
        // ****     11110000    0xf0
        // *        10000000    0x80
        // ****     11110000    0xf0
        // *        10000000    0x80
        // *        10000000    0x80
        // Each sprite is a max of 15 bytes in size, each byte is one row.

        public static readonly byte[] Zero  = new byte[] { 0xf0, 0x90, 0x90, 0x90, 0xf0 };
        public static readonly byte[] One   = new byte[] { 0x20, 0x60, 0x20, 0x20, 0x70 };
        public static readonly byte[] Two   = new byte[] { 0xf0, 0x10, 0xf0, 0x80, 0xf0 };
        public static readonly byte[] Three = new byte[] { 0xf0, 0x10, 0xf0, 0x10, 0xf0 };
        public static readonly byte[] Four  = new byte[] { 0x90, 0x90, 0xf0, 0x10, 0x10 };
        public static readonly byte[] Five  = new byte[] { 0xf0, 0x80, 0xf0, 0x10, 0xf0 };
        public static readonly byte[] Six   = new byte[] { 0xf0, 0x80, 0xf0, 0x90, 0xf0 };
        public static readonly byte[] Seven = new byte[] { 0xf0, 0x10, 0x20, 0x40, 0x40 };
        public static readonly byte[] Eight = new byte[] { 0xf0, 0x90, 0xf0, 0x90, 0xf0 };
        public static readonly byte[] Nine  = new byte[] { 0xf0, 0x90, 0xf0, 0x10, 0xf0 };
        public static readonly byte[] A     = new byte[] { 0xf0, 0x90, 0xf0, 0x90, 0x90 };
        public static readonly byte[] B     = new byte[] { 0xe0, 0x90, 0xe0, 0x90, 0xe0 };
        public static readonly byte[] C     = new byte[] { 0xf0, 0x80, 0x80, 0x80, 0xf0 };
        public static readonly byte[] D     = new byte[] { 0xe0, 0x90, 0x90, 0x90, 0xe0 };
        public static readonly byte[] E     = new byte[] { 0xf0, 0x80, 0xf0, 0x80, 0xf0 };
        public static readonly byte[] F     = new byte[] { 0xf0, 0x80, 0xf0, 0x80, 0x80 };
    }
}
