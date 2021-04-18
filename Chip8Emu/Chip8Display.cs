using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chip8Emu
{
    public partial class Chip8Display : Form
    {
        private readonly Stopwatch Stopwatch500Hz = Stopwatch.StartNew();
        private readonly TimeSpan ElapsedTimeTarget500Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 500);
        private readonly Stopwatch Stopwatch60Hz = Stopwatch.StartNew();
        private readonly TimeSpan ElapsedTimeTarget60Hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

        private readonly Chip8 Chip8;
        private readonly Bitmap DisplayBitmap;

        public Chip8Display()
        {
            InitializeComponent();

            DisplayBitmap = new Bitmap(64, 32);

            Chip8 = new Chip8(Draw);
            Chip8.LoadProgram(File.ReadAllBytes(@"C:\dev\Chip8Emu\test_opcode.ch8"));
        }

        Task ExecutionLoop()
        {
            while (true)
            {
                if (Stopwatch500Hz.Elapsed >= ElapsedTimeTarget500Hz)
                {
                    Chip8.Tick();
                }
                if (Stopwatch60Hz.Elapsed >= ElapsedTimeTarget60Hz)
                {

                }
            }
        }

        void Draw(bool[,] displayBuffer)
        {
            var bitmapData = DisplayBitmap.LockBits(new Rectangle(0, 0, DisplayBitmap.Width, DisplayBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* pixelPtr = (byte*)bitmapData.Scan0;

                for (var x = 0; x < DisplayBitmap.Width; x++)
                {
                    for (var y = 0; y < DisplayBitmap.Height; y++)
                    {
                        var colorValue = (byte)(displayBuffer[x, y] ? 255 : 0);
                        pixelPtr[0] = colorValue; // B
                        pixelPtr[1] = colorValue; // G
                        pixelPtr[2] = colorValue; // R
                        pixelPtr[3] = 255;        // A

                        pixelPtr += 4; // 4 bytes used for RGBA
                    }
                }
            }

            DisplayBitmap.UnlockBits(bitmapData);
            this.
        }

        private void Chip8Display_Load(object sender, EventArgs e)
        {
            Task.Run(ExecutionLoop);
        }

        void Foo()
        {
            var bitmap = new Bitmap(64, 32);
            Graphics bitmapGraphics = Graphics.FromImage(bitmap);

            bitmapGraphics.FillRectangle(Brushes.Red, 0, 0, 5, 5);
            bitmapGraphics.FillRectangle(Brushes.Blue, 5, 5, 5, 5);

            //DisplayPictureBox.Image = bitmap;
        }
    }
}
