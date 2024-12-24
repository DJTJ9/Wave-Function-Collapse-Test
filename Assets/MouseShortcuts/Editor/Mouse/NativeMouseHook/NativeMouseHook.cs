namespace Kamgam.NativeMouseHookLib
{
    public enum NativeMouseEvent
    {
        LeftDown = 0,
        LeftUp = 1,

        RightDown = 2,
        RightUp = 3,

        MiddleDown = 4,
        MiddleUp = 5,

        WheelTurned = 6,

        FourthDown = 7,
        FourthUp = 8,

        FifthDown = 9,
        FifthUp = 10,
    }

    public static partial class NativeMouseHook
    {
        public static System.Action<NativeMouseEvent> MouseEvent;

        public static bool IsMouseDownEvent(NativeMouseEvent evt)
        {
            return evt == NativeMouseEvent.LeftDown
                || evt == NativeMouseEvent.RightDown
                || evt == NativeMouseEvent.MiddleDown
                || evt == NativeMouseEvent.FourthDown
                || evt == NativeMouseEvent.FifthDown;
        }

        public static bool IsMouseUpEvent(NativeMouseEvent evt)
        {
            return evt == NativeMouseEvent.LeftUp
                || evt == NativeMouseEvent.RightUp
                || evt == NativeMouseEvent.MiddleUp
                || evt == NativeMouseEvent.FourthUp
                || evt == NativeMouseEvent.FifthUp;
        }

        public static bool IsMouseThirdPlusDownEvent(NativeMouseEvent evt)
        {
            return evt == NativeMouseEvent.MiddleDown
                || evt == NativeMouseEvent.FourthDown
                || evt == NativeMouseEvent.FifthDown;
        }

        public static bool IsMouseThirdPlusUpEvent(NativeMouseEvent evt)
        {
            return 
                   evt == NativeMouseEvent.MiddleUp
                || evt == NativeMouseEvent.FourthUp
                || evt == NativeMouseEvent.FifthUp;
        }
    }
}