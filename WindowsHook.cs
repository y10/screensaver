using System.Runtime.InteropServices;

namespace Screensaver
{
    public class WindowsHook : IDisposable
    {
        public event EventHandler<KeyEventArgs> KeyDown;
        public event EventHandler<KeyEventArgs> KeyUp;
        public event EventHandler MouseClick;

        public delegate int CallbackDelegate(int Code, int W, int L);


        [DllImport("user32", CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(HookType idHook, CallbackDelegate lpfn, int hInstance, int threadId);

        [DllImport("user32", CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32", CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, int wParam, int lParam);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GetCurrentThreadId();

        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        private int KeyboardHookID = 0;
        private int MouseHookID = 0;

        public WindowsHook()
        {
            KeyboardHookID = SetWindowsHookEx(
                HookType.WH_KEYBOARD_LL, 
                new CallbackDelegate(KeybHookProc),
                0, //0 for local hook. eller hwnd til user32 for global
                0); //0 for global hook. eller thread for hooken

            MouseHookID = SetWindowsHookEx(
                HookType.WH_MOUSE_LL,
                new CallbackDelegate(MouseHookProc),
                0, //0 for local hook. eller hwnd til user32 for global
                0); //0 for global hook. eller thread for hooken
        }

        private int MouseHookProc(int nCode, int wParam, int lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
            {
                MouseClick(this, EventArgs.Empty);
            }

            return CallNextHookEx(MouseHookID, nCode, wParam, lParam);
        }

        private int KeybHookProc(int Code, int W, int L)
        {
            if (Code < 0)
            {
                return CallNextHookEx(KeyboardHookID, Code, W, L);
            }

            KeyEvents kEvent = (KeyEvents)W;

            if (kEvent != KeyEvents.KeyDown && kEvent != KeyEvents.KeyUp && kEvent != KeyEvents.SKeyDown && kEvent != KeyEvents.SKeyUp)
            {
            }
            if (kEvent == KeyEvents.KeyDown || kEvent == KeyEvents.SKeyDown)
            {
                if (KeyDown != null) KeyDown(this, new KeyEventArgs((Keys)W));
            }
            if (kEvent == KeyEvents.KeyUp || kEvent == KeyEvents.SKeyUp)
            {
                if (KeyUp != null) KeyUp(this, new KeyEventArgs((Keys)W));
            }

            return CallNextHookEx(KeyboardHookID, Code, W, L);
        }

        public enum KeyEvents
        {
            KeyDown = 0x0100,
            KeyUp = 0x0101,
            SKeyDown = 0x0104,
            SKeyUp = 0x0105
        }

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [DllImport("user32.dll")]
        static public extern short GetKeyState(System.Windows.Forms.Keys nVirtKey);

        public static bool GetCapslock()
        {
            return Convert.ToBoolean(GetKeyState(System.Windows.Forms.Keys.CapsLock)) & true;
        }
        public static bool GetNumlock()
        {
            return Convert.ToBoolean(GetKeyState(System.Windows.Forms.Keys.NumLock)) & true;
        }
        public static bool GetScrollLock()
        {
            return Convert.ToBoolean(GetKeyState(System.Windows.Forms.Keys.Scroll)) & true;
        }
        public static bool GetShiftPressed()
        {
            int state = GetKeyState(System.Windows.Forms.Keys.ShiftKey);
            if (state > 1 || state < -1) return true;
            return false;
        }
        public static bool GetCtrlPressed()
        {
            int state = GetKeyState(System.Windows.Forms.Keys.ControlKey);
            if (state > 1 || state < -1) return true;
            return false;
        }
        public static bool GetAltPressed()
        {
            int state = GetKeyState(System.Windows.Forms.Keys.Menu);
            if (state > 1 || state < -1) return true;
            return false;
        }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                UnhookWindowsHookEx(KeyboardHookID);
                UnhookWindowsHookEx(MouseHookID);

                disposedValue = true;
            }
        }

        ~WindowsHook()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
