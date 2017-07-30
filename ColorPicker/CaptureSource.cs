using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ColorPicker.WinApi;

namespace ColorPicker
{
    public class CaptureSource : IDisposable
    {
        private readonly IntPtr _hScreenDc;

        private readonly IntPtr _hDestMemDc;
        private readonly IntPtr _hDestBitmap;

        public int Width { get; }
        public int Height { get; }

        public CaptureSource(int width, int height)
        {
            Width = width;
            Height = height;

            _hScreenDc = User32.GetDc(IntPtr.Zero);

            _hDestMemDc = Gdi32.CreateCompatibleDc(_hScreenDc);
            _hDestBitmap = Gdi32.CreateCompatibleBitmap(_hScreenDc, Width, Height);

            if (Gdi32.SelectObject(_hDestMemDc, _hDestBitmap) == IntPtr.Zero)
                throw new Win32Exception();
        }

        public (BitmapSource previewBitmap, Color selectedColor) CaptureAtPosition(int x, int y)
        {
            int captureX = x - Width / 2;
            int captureY = y - Height / 2;

            if (!Gdi32.BitBlt(_hDestMemDc, 0, 0, Width, Height, _hScreenDc, captureX, captureY, Gdi32.SrcCopy))
                throw new Win32Exception();

            BitmapSource previewBitmap = Imaging.CreateBitmapSourceFromHBitmap(_hDestBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            uint colorValue = Gdi32.GetPixel(_hScreenDc, x, y);
            Color selectedColor = Color.FromRgb((byte)(colorValue >> 0), (byte)(colorValue >> 8), (byte)(colorValue >> 16));

            return (previewBitmap, selectedColor);
        }

        public void Dispose()
        {
            User32.ReleaseDc(_hScreenDc, IntPtr.Zero);

            Gdi32.DeleteDc(_hDestMemDc);
            Gdi32.DeleteObject(_hDestBitmap);
        }
    }
}