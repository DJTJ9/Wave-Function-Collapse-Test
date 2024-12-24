#if UNITY_EDITOR
using Kamgam.NativeMouseHookLib;
using System.Collections.Generic;

namespace Kamgam.MouseShortcuts
{
    [System.Serializable]
    public class MouseShortcutsData
    {
        [System.Serializable]
        public class MouseEventShortcut
        {
            public NativeMouseEvent MouseEvent;
            public string CommandId;

            public MouseEventShortcut(NativeMouseEvent mouseEvent, string commandId)
            {
                MouseEvent = mouseEvent;
                CommandId = commandId;
            }
        }

        public List<MouseEventShortcut> Shortcuts;

        public void CreateOrFillData()
        {
            if (GetCommandId(NativeMouseHookLib.NativeMouseEvent.MiddleUp) == null)
                AddOrUpdateShortcut(NativeMouseHookLib.NativeMouseEvent.MiddleUp, MouseCommands.CommandIdDoNothing);

            if (GetCommandId(NativeMouseHookLib.NativeMouseEvent.FourthUp) == null)
                AddOrUpdateShortcut(NativeMouseHookLib.NativeMouseEvent.FourthUp, MouseCommands.CommandIdSelectionPrevious);

            if (GetCommandId(NativeMouseHookLib.NativeMouseEvent.FifthUp) == null)
                AddOrUpdateShortcut(NativeMouseHookLib.NativeMouseEvent.FifthUp, MouseCommands.CommandIdSelectionNext);

            /*
            if (GetCommandId(NativeMouseHookLib.NativeMouseEvent.WheelTurned) == null)
                AddOrUpdateShortcut(NativeMouseHookLib.NativeMouseEvent.WheelTurned, MouseCommands.CommandIdDoNothing);
            */
        }

        public string GetCommandId(NativeMouseEvent evt)
        {
            var shortcut = getShortcut(evt);
            if (shortcut != null)
                return shortcut.CommandId;

            return null;
        }

        public void AddOrUpdateShortcut(NativeMouseEvent evt, string commandId)
        {
            var shortcut = getShortcut(evt);
            if (shortcut != null)
            { 
                shortcut.CommandId = commandId;
            }
            else
            {
                Shortcuts.Add(
                    new MouseEventShortcut(evt, commandId)
                );
            }
        }

        protected MouseEventShortcut getShortcut(NativeMouseEvent evt)
        {
            if (Shortcuts == null)
            {
                Shortcuts = new List<MouseEventShortcut>();
                return null;
            }

            for (int i = 0; i < Shortcuts.Count; i++)
                if (Shortcuts[i].MouseEvent == evt)
                    return Shortcuts[i];

            return null;
        }
    }
}
#endif