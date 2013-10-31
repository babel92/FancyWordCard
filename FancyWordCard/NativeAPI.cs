using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace FancyWordCard
{
    class NativeAPI
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;      // width of left border that retains its size 
            public int cxRightWidth;     // width of right border that retains its size 
            public int cyTopHeight;      // height of top border that retains its size 
            public int cyBottomHeight;   // height of bottom border that retains its size
        };


        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")]
        static extern bool DwmIsCompositionEnabled();
        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr Handle, int x, int y, int w, int h, bool repaint);
    }
}
