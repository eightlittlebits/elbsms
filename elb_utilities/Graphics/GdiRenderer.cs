using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using elb_utilities.NativeMethods;

namespace elb_utilities.Graphics
{
    public class GdiRenderer
    {
        private Control _renderControl;

        private int _controlWidth, _controlHeight;

        public GdiRenderer(Control renderControl)
        {
            _renderControl = renderControl;

            _renderControl.Resize += RenderControlResized;

            _controlWidth = renderControl.ClientSize.Width;
            _controlHeight = renderControl.ClientSize.Height;
        }

        private void RenderControlResized(object sender, EventArgs e)
        {
            var control = (Control)sender;

            _controlWidth = control.ClientSize.Width;
            _controlHeight = control.ClientSize.Height;
        }

        public void DrawBitmap(Bitmap bitmap)
        {
            using (var grDest = System.Drawing.Graphics.FromHwnd(_renderControl.Handle))
            using (var grSrc = System.Drawing.Graphics.FromImage(bitmap))
            {
                IntPtr hdcDest = IntPtr.Zero;
                IntPtr hdcSrc = IntPtr.Zero;
                IntPtr hBitmap = IntPtr.Zero;
                IntPtr hOldObject = IntPtr.Zero;

                try
                {
                    hdcDest = grDest.GetHdc();
                    hdcSrc = grSrc.GetHdc();
                    hBitmap = bitmap.GetHbitmap();

                    hOldObject = Gdi32.SelectObject(hdcSrc, hBitmap);
                    if (hOldObject == IntPtr.Zero)
                        throw new Win32Exception();
                    if (!Gdi32.StretchBlt(hdcDest, 0, 0, _controlWidth, _controlHeight,
                                            hdcSrc, 0, 0, bitmap.Width, bitmap.Height,
                                            Gdi32.TernaryRasterOperations.SRCCOPY))
                        throw new Win32Exception();
                }
                finally
                {
                    if (hOldObject != IntPtr.Zero) Gdi32.SelectObject(hdcSrc, hOldObject);
                    if (hBitmap != IntPtr.Zero) Gdi32.DeleteObject(hBitmap);
                    if (hdcDest != IntPtr.Zero) grDest.ReleaseHdc(hdcDest);
                    if (hdcSrc != IntPtr.Zero) grSrc.ReleaseHdc(hdcSrc);
                }
            }
        }
    }
}