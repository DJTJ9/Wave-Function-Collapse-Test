#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using Kamgam.NativeMouseHookLib;
using System;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace Kamgam.MouseShortcuts
{
    public class MouseShortcuts
    {
        #region singleton
        private static MouseShortcuts _instance;

        public static MouseShortcuts Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MouseShortcuts();

                return _instance;
            }
        }

        private void noOp() { }
        #endregion

        public MouseCommands Commands;

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            Instance.noOp();
        }

        private MouseShortcuts()
        {
            MouseShortcutsSettings.GetOrCreateSettings();

            Commands = new MouseCommands();

            NativeMouseHook.Install();
            NativeMouseHook.MouseEvent -= onMouseEvent;
            NativeMouseHook.MouseEvent += onMouseEvent;

            ActionHistory.Instance.Init();
        }

        ~MouseShortcuts()
        {
            NativeMouseHook.MouseEvent -= onMouseEvent;
            NativeMouseHook.Uninstall();
        }

        private void onMouseEvent(NativeMouseEvent evt)
        {
            try
            {
                if (isMouseEventValid(evt))
                {
                    var settings = MouseShortcutsSettings.GetOrCreateSettings();
                    var commandId = settings.Data.GetCommandId(evt);
                    Commands.Invoke(commandId);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Debug.LogError(e.InnerException.Message + "\n" + e.InnerException.StackTrace);
                }
                else
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        protected double lastMouseEventTime = -1000f;
        protected NativeMouseEvent? lastMouseEvent = null;

        private bool isMouseEventValid(NativeMouseEvent evt)
        {
            if (
                       evt == NativeMouseEvent.LeftDown
                    || evt == NativeMouseEvent.LeftUp
                    || evt == NativeMouseEvent.RightDown
                    || evt == NativeMouseEvent.RightUp
                    || evt == NativeMouseEvent.MiddleDown
                    || evt == NativeMouseEvent.MiddleUp
                    || evt == NativeMouseEvent.FourthDown
                    || evt == NativeMouseEvent.FourthUp
                    || evt == NativeMouseEvent.FifthDown
                    || evt == NativeMouseEvent.FifthUp
                )
            {
                // If enough time elapsed then skip the same button check
                if (EditorApplication.timeSinceStartup - lastMouseEventTime > 0.1f)
                {
                    lastMouseEventTime = EditorApplication.timeSinceStartup;
                    lastMouseEvent = evt;
                    return true;
                }
                lastMouseEventTime = EditorApplication.timeSinceStartup;

                // No info on the last event, assume it's valid.
                if (!lastMouseEvent.HasValue)
                {
                    lastMouseEvent = evt;
                    return true;
                }

                // After a down event there has to come an up event (except if it's a double click).
                if (lastMouseEvent.Value == evt)
                    return false;
            }

            lastMouseEventTime = EditorApplication.timeSinceStartup;
            lastMouseEvent = evt;
            return true;
        }
    }
}
#endif
