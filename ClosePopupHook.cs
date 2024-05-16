using System.Runtime.InteropServices;
using System.Text;

namespace CadConversion
{
    class ClosePopupHook : IDisposable
    {
        #region Constants and DLL Imports
        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        private const uint WINEVENT_OUTOFCONTEXT = 0;

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
        
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion

        #region Delegates
        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        #endregion

        #region Private Fields
        private bool _disposed = false;
        private IntPtr _hookID = IntPtr.Zero;
        private readonly WinEventDelegate _procDelegate;
        private static readonly List<string> knownPopupIdentifiers =
        [
            "You need the full version of eDrawings to perform this function.",
            "Error reading file.",
        ];
        #endregion

        #region Constructor and Initialization
        public ClosePopupHook()
        {
            _procDelegate = new WinEventDelegate(WinEventProc);
            _hookID = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
        }
        #endregion

        #region Event Handling
        private static bool EnumWindowProc(IntPtr hWnd, IntPtr lParam)
        {
            StringBuilder text = new(255);
            _ = GetWindowText(hWnd, text, 255);
            string childWindowText = text.ToString();
            Console.WriteLine("Popup Hook: Child window text: " + childWindowText);
            if (knownPopupIdentifiers.Any(identifier => childWindowText.Contains(identifier, StringComparison.OrdinalIgnoreCase)))
            {
                SetForegroundWindow(hWnd);
                PressTabKey();
                PressTabKey();
                PressEnterKey();
                return false; // stop enumerating windows
            }
            return true; // Continue enumerating other child windows
        }

        private static void PressTabKey()
        {
            keybd_event(0x09, 0, 0, UIntPtr.Zero); // VK_TAB down
            keybd_event(0x09, 0, 0x0002, UIntPtr.Zero); // VK_TAB up
        }
        private static void PressEnterKey()
        {
            keybd_event(0x0D, 0, 0, UIntPtr.Zero); // VK_RETURN down
            keybd_event(0x0D, 0, 0x0002, UIntPtr.Zero); // VK_RETURN up
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            Console.WriteLine("Popup Hook: A new window is now in the foreground.");
            StringBuilder windowText = new(256);
            if (GetWindowText(hwnd, windowText, 256) > 0)
            {
                Console.WriteLine($"Popup Hook: Window title: {windowText}");
                if (windowText.ToString() == "eDrawings")
                {
                    EnumChildWindows(hwnd, new EnumWindowsProc(EnumWindowProc), IntPtr.Zero);
                }
            }
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_hookID != IntPtr.Zero)
                {
                    UnhookWinEvent(_hookID);
                    _hookID = IntPtr.Zero;
                }
                _disposed = true;
            }
        }

        ~ClosePopupHook()
        {
            Dispose(false);
        }
        #endregion
    }
}
