using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace NinjaTools.GUI.Wpf
{
    public class FullScreenManager
    { 
        private readonly Window _window;
        private WindowStyle _windowStyle;
        private ResizeMode _resizeMode;

        public bool IsFullscreen { get; private set; }

        public FullScreenManager(Window window)
        {
            _window = window;
            _window.Deactivated += OnDeactivated;
            _window.Activated   += OnActivated;
        }

        private void OnDeactivated(object sender, EventArgs e)
        {
            if (IsFullscreen)
                WndTools.ShowTaskbar();
        }

        private void OnActivated(object sender, EventArgs e)
        {
            if (IsFullscreen)
            {
                WndTools.SetWinFullScreen(new WindowInteropHelper(_window).Handle);
                WndTools.HideTaskbar();
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (IsFullscreen)
                WndTools.ShowTaskbar();
        }

        public void Toggle()
        {
            if (IsFullscreen)
            {
                IsFullscreen = false;

                bool isActive = _window.IsActive;

                _window.WindowStyle = _windowStyle;
                _window.ResizeMode  = _resizeMode;
                WndTools.ShowTaskbar();
                _window.Closing   -= OnClosing;
                //_window.Topmost = false;

                if (isActive)
                    _window.Activate();

            }
            else
            {
                IsFullscreen = true;

                _windowStyle = _window.WindowStyle;
                _resizeMode  = _window.ResizeMode;
                _window.WindowStyle = WindowStyle.None;
                _window.ResizeMode  = ResizeMode.NoResize;
                WndTools.HideTaskbar();
                _window.Activated += OnActivated;
                _window.Closing   += OnClosing;
                //this.Topmost = true;

                if(new WindowInteropHelper(_window).Handle != IntPtr.Zero)
                    WndTools.SetWinFullScreen(new WindowInteropHelper(_window).Handle);
            }
        }
        

        internal class WndTools
        {
            [DllImport("user32.dll")]
            private static extern int FindWindow(string className, string windowText);
            [DllImport("user32.dll")]
            private static extern int ShowWindow(int hwnd, int command);

            private const int SW_HIDE = 0;
            private const int SW_SHOW = 1;

            public static void HideTaskbar()
            {

                int hwnd = FindWindow("Shell_TrayWnd", "");
                ShowWindow(hwnd, SW_HIDE);
            }

            public static void ShowTaskbar()
            {

                int hwnd = FindWindow("Shell_TrayWnd", "");
                ShowWindow(hwnd, SW_SHOW);
            }

            [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
            public static extern int GetSystemMetrics(int which);

            [DllImport("user32.dll")]
            public static extern void
                SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
                             int    X,    int    Y, int width, int height, uint flags);

            private const  int    SM_CXSCREEN    = 0;
            private const  int    SM_CYSCREEN    = 1;
            private static IntPtr HWND_TOP       = IntPtr.Zero;
            private const  int    SWP_SHOWWINDOW = 64; // 0x0040

            public static int ScreenX
            {
                get { return GetSystemMetrics(SM_CXSCREEN); }
            }

            public static int ScreenY
            {
                get { return GetSystemMetrics(SM_CYSCREEN); }
            }

            public static void SetWinFullScreen(IntPtr hwnd)
            {
                SetWindowPos(hwnd, HWND_TOP, 0, 0, ScreenX, ScreenY, SWP_SHOWWINDOW);
            }
        }


        public void DisableFullscreen()
        {
            if(IsFullscreen)
                Toggle();
        }
        public void SetFullscreen()
        {
            if (!IsFullscreen)
                Toggle();
        }
    }

}
