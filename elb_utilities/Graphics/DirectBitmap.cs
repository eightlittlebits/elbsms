using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace elb_utilities.Graphics
{
    public class DirectBitmap : IDisposable
    {
        public int Width => Bitmap.Width;
        public int Height => Bitmap.Height;

        public Bitmap Bitmap { get; private set; }
        public IntPtr BitmapData { get; private set; }

        public DirectBitmap(int width, int height)
        {
            BitmapData = Marshal.AllocHGlobal(width * height * sizeof(uint));
            Bitmap = new Bitmap(width, height, width * 4, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, BitmapData);
        }

        ~DirectBitmap()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (Bitmap != null)
                {
                    Bitmap.Dispose();
                    Bitmap = null;
                }
            }

            // free unmanaged resources
            if (BitmapData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(BitmapData);
                BitmapData = IntPtr.Zero;
            }
        }
    }
}
