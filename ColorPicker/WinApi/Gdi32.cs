using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorPicker.WinApi
{
    public static class Gdi32
    {
        public const int SrcCopy = 0x00CC0020;

        [DllImport("gdi32", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDc(IntPtr hDc);

        [DllImport("gdi32", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDc, int nWidth, int nHeight);

        [DllImport("gdi32", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hDc, IntPtr hObject);

        [DllImport("gdi32", EntryPoint = "BitBlt")]
        public static extern bool BitBlt(IntPtr hDestDc, int x, int y, int nWidth, int nHeight, IntPtr hSrcDc, int srcX, int srcY, int rop);

        [DllImport("gdi32", EntryPoint = "GetPixel")]
        public static extern uint GetPixel(IntPtr hDc, int nXPos, int nYPos);

        [DllImport("gdi32", EntryPoint = "DeleteDC")]
        public static extern IntPtr DeleteDc(IntPtr hDc);

        [DllImport("gdi32", EntryPoint = "DeleteObject")]
        public static extern IntPtr DeleteObject(IntPtr hObject);
    }
}
