#if UNITY_EDITOR_WIN
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kamgam.NativeMouseHookLib
{
    public static partial class NativeMouseHook
    {
        public static bool IsSupported = true;

        private static HookMessageDelegate callback = MessageHookCallback;
        private static IntPtr hookID = IntPtr.Zero;

        public static void Install()
        {
            hookID = SetHook(callback);
        }

        public static void Uninstall()
        {
            UnhookWindowsHookEx(hookID);
        }

        private static IntPtr SetHook(HookMessageDelegate callback)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                uint threadId = GetCurrentThreadId();

                // See https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa
                return SetWindowsHookEx(
                    WH_GETMESSAGE, // Thou shall not use WH_MOUSE_LL ;), but WH_MOUSE is missing 4th an 5th button infos, thus WH_GETMESSAGE
                    callback,      // The callback procedure is within the same process.
                    IntPtr.Zero,   // The hMod parameter must be set to NULL if the dwThreadId parameter specifies a thread
                                   // created by the current process and if the hook procedure is within the code associated
                                   // with the current process.
                    threadId       // We do specify a native thread id.
                    ); 
            }
        }

        /// <summary>
        /// Message proc callback.<br />
        /// see: https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644981(v=vs.85)
        /// </summary>
        /// <param name="nCode">Specifies whether the hook procedure must process the message.<br />
        /// If code is HC_ACTION, the hook procedure must process the message. If code is less than zero, 
        /// the hook procedure must pass the message to the CallNextHookEx function without further processing and 
        /// should return the value returned by CallNextHookEx.<br /><br />
        /// 
        /// HC_ACTION 0 = The wParam and lParam parameters contain information about a mouse message.<br />
        /// HC_NOREMOVE = 3 The wParam and lParam parameters contain information about a mouse message, and the mouse message has not been removed from the message queue.
        /// </param>
        /// <param name="wParam">Specifies whether the message has been removed from the queue. This parameter can be one of the following values.</param>
        /// <param name="lParam">A pointer to a MSG struct, see: https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-msg?redirectedfrom=MSDN</param>
        /// <returns></returns>
        private delegate IntPtr HookMessageDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr MessageHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                Msg msg = Marshal.PtrToStructure<Msg>(lParam);
                MessageIdentifier msgId = (MessageIdentifier) msg.message;
                switch (msgId)
                {
                    case MessageIdentifier.WM_LBUTTONDOWN:
                        MouseEvent?.Invoke(NativeMouseEvent.LeftDown);
                        break;

                    case MessageIdentifier.WM_LBUTTONUP:
                        MouseEvent?.Invoke(NativeMouseEvent.LeftUp);
                        break;

                    // Performance hog, not needed.
                    //case MessageIdentifier.WM_MOUSEMOVE:
                    //    MouseEvent?.Invoke(MouseButtonEvent.LeftUp);
                    //    break;

                    case MessageIdentifier.WM_MOUSEWHEEL:
                        MouseEvent?.Invoke(NativeMouseEvent.WheelTurned);
                        break;

                    case MessageIdentifier.WM_MBUTTONDOWN:
                        MouseEvent?.Invoke(NativeMouseEvent.MiddleDown);
                        break;

                    case MessageIdentifier.WM_MBUTTONUP:
                        MouseEvent?.Invoke(NativeMouseEvent.MiddleUp);
                        break;

                    case MessageIdentifier.WM_RBUTTONDOWN:
                        MouseEvent?.Invoke(NativeMouseEvent.RightDown);
                        break;

                    case MessageIdentifier.WM_RBUTTONUP:
                        MouseEvent?.Invoke(NativeMouseEvent.RightUp);
                        break;

                    case MessageIdentifier.WM_XBUTTONDOWN:
                        var downBtn = HighWord((uint)msg.wParam);
                        if (downBtn == XBUTTON1)
                            MouseEvent?.Invoke(NativeMouseEvent.FourthDown);
                        else if (downBtn == XBUTTON2)
                            MouseEvent?.Invoke(NativeMouseEvent.FifthDown);
                        break;

                    case MessageIdentifier.WM_XBUTTONUP:
                        var upBtn = HighWord((uint)msg.wParam);
                        if (upBtn == XBUTTON1)
                            MouseEvent?.Invoke(NativeMouseEvent.FourthUp);
                        else if (upBtn == XBUTTON2)
                            MouseEvent?.Invoke(NativeMouseEvent.FifthUp);
                        break;
                }
            }

            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        public static ushort LowWord(uint val)
        {
            return (ushort)val;
        }

        public static ushort HighWord(uint val)
        {
            return (ushort)(val >> 16);
        }

        /// <summary>
        /// High level mouse events (for one application or thread).<br />
        /// Was used initially but it seems the information to differentiate
        /// between WM_XBUTTONDOWN buttons was missing. Thus now WH_GETMESSAGE
        /// is used because the MSG struct contains that info.
        /// </summary>
        private const int WH_MOUSE = 7;

        // Message events for the window.
        private const int WH_GETMESSAGE = 3;

        /// <summary>
        /// Mouse Input Notifications<br />
        /// see: https://docs.microsoft.com/en-us/windows/win32/inputdev/mouse-input-notifications
        /// </summary>
        private enum MessageIdentifier
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_XBUTTONDOWN = 0x020B,
            WM_XBUTTONUP = 0x020C
        }

        /// <summary>
        /// See: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-get_xbutton_wparam<br />
        /// Actually the documentation there is WRONG. The GET_XBUTTON_WPARAM = HIWORD = returns a WORD (unsigned int 16). Specifically the high word ot he given wParam.<br />
        /// </summary>
        private const short XBUTTON1 = 0x0001;
        private const short XBUTTON2 = 0x0002;

        /// <summary>
        /// The MSG structure which lParam points to.<br />
        /// See: https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mousehookstruct
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Msg
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public short time;
            public Point pt;
            public short lPrivate;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// See https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
          HookMessageDelegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
    }
}
#endif