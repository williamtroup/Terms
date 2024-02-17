using System;
using System.Windows;
using System.Windows.Interop;

namespace Terms.UI.Tools.Actions
{
    public class LockWindowActions
    {
        #region Private Constants

        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MOVE = 0xF010;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;

        #endregion

        #region Private Read-Only Variables

        private readonly Window m_window;

        #endregion

        public static LockWindowActions JustMaximized(Window window)
        {
            return new LockWindowActions(window)
            {
                LockMaximizing = true
            };
        }

        public LockWindowActions(Window window)
        {
            m_window = window;

            m_window.SourceInitialized += Window_OnSourceInitialized;
        }

        public bool LockMoving { get; set; }
        public bool LockMinimizing { private get; set; }
        public bool LockMaximizing { private get; set; }

        #region Private "WndProc" Helpers

        private void Hook()
        {
            WindowInteropHelper helper = new(m_window);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);

            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_SYSCOMMAND:

                    int command = wParam.ToInt32() & 0xfff0;

                    switch (command)
                    {
                        case SC_MOVE when LockMoving:
                            handled = true;
                            break;

                        case SC_MINIMIZE when LockMinimizing || !m_window.ShowInTaskbar:
                            handled = true;
                            break;

                        case SC_MAXIMIZE when LockMaximizing || !m_window.ShowInTaskbar:
                            handled = true;
                            break;
                    }

                    break;
            }

            return IntPtr.Zero;
        }

        #endregion

        #region PRivate "Window" Events

        private void Window_OnSourceInitialized(object sender, EventArgs e)
        {
            Hook();
        }

        #endregion
    }
}