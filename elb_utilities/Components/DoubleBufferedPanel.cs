using System.Windows.Forms;

namespace elb_utilities.Components
{
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            // these settings as applied below conflict with the documents,
            // we're ok in this instance though as we're rendering the control
            // ourselves constantly. We don't need windows to be raising either
            // the WM_ERASEBKGND or WM_PAINT events. Seetting 
            // AllPaintingInWmPaint to true prevents WM_ERASEBKGND and setting
            // UserPaint to false prevents WM_PAINT
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, false);

            UpdateStyles();
        }
    }
}
