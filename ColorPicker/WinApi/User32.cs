using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorPicker.WinApi
{
    public static class User32
    {
        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDc(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDc(IntPtr hWnd, IntPtr hDc);
    }
}