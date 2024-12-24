#if UNITY_EDITOR
using Kamgam.NativeMouseHookLib;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kamgam.MouseShortcuts
{
    /// <summary>
    /// This history tracks the changes in focused EditorWindows.
    /// </summary>
    public class EditorWindowHistory : IHistory
    {
        public class Entry : IEntryWithTime
        {
            public double Time;
            public EditorWindow EditorWindow;

            public Entry(double time, EditorWindow editorWindow)
            {
                Time = time;
                EditorWindow = editorWindow;
            }

            public double GetTime()
            {
                return Time;
            }
        }

        int maxEntries;
        bool ignoreNextChange;
        List<Entry> entries;
        Action<IHistory, double> _onNewEntryAdded;
        Action<IHistory, double> _onEntryRestored;

        EditorWindow lastFocusedWindow;
        int everyNthUpdate;
        int updateCounter;

        /// <summary>
        /// Is any mouse button pressed down? Left and right mouse buttons are not taken into account.
        /// </summary>
        bool mouseButtonThirdPlusDown = false;

        public void Init(Action<IHistory, double> onNewEntryAdded, Action<IHistory, double> onEntryRestored)
        {
            _onNewEntryAdded = onNewEntryAdded;
            _onEntryRestored = onEntryRestored;

            maxEntries = 20;
            entries = new List<Entry>(maxEntries);

            lastFocusedWindow = EditorWindow.focusedWindow;
            everyNthUpdate = 10;
            updateCounter = everyNthUpdate;

            EditorApplication.update -= onUpdate;
            EditorApplication.update += onUpdate;

            Selection.selectionChanged -= onSelectionChanged;
            Selection.selectionChanged += onSelectionChanged;

            NativeMouseHook.MouseEvent += onMouseEvent;
        }

        public void RemoveRecentlyAddedEntries(double ageLimitInSec)
        {
            double time = EditorApplication.timeSinceStartup;
            for (int i = entries.Count-1; i >= 0; i--)
            {
                if (time - entries[i].Time < ageLimitInSec)
                    entries.RemoveAt(i);
            }
        }

        public void UpdateLastFocusedWindow()
        {
            lastFocusedWindow = EditorWindow.focusedWindow;
        }

        void onMouseEvent(NativeMouseEvent evt)
        {
            try
            {
                if (NativeMouseHook.IsMouseThirdPlusDownEvent(evt))
                {
                    mouseButtonThirdPlusDown = true;
                }
                else if(NativeMouseHook.IsMouseThirdPlusUpEvent(evt))
                {
                    mouseButtonThirdPlusDown = false;
                }
            }
            catch { };
        }

        public void DeInit()
        {
            EditorApplication.update -= onUpdate;
            Selection.selectionChanged -= onSelectionChanged;
        }

        void onUpdate()
        {
            if (--updateCounter < 0)
            {
                updateCounter = everyNthUpdate;
                if (lastFocusedWindow != EditorWindow.focusedWindow)
                {
                    lastFocusedWindow = EditorWindow.focusedWindow;

                    // Skip recording editor window changes if disabled in the settings.
                    var settings = MouseShortcutsSettings.GetOrCreateSettings();
                    if (!settings.TrackWindows)
                        return;

                    // Any mouse down event triggers a refocusing of the EditorWindow below.
                    // To avoid recording these we ony record if no mouse button is pressed.
                    if (!mouseButtonThirdPlusDown)
                        addEntry(lastFocusedWindow);
                }
            }
        }

        void onSelectionChanged()
        {
            if (lastFocusedWindow != EditorWindow.focusedWindow)
            {
                lastFocusedWindow = EditorWindow.focusedWindow;
            }
        }

        public double GetNextTimeDelta(double referenceTime)
        {
            var entry = ActionHistory.GetClosestEntry(entries, referenceTime, next: true);
            if (entry == null)
                return -1;
            else
                return entry.GetTime() - referenceTime;
        }

        public double GetPreviousTimeDelta(double referenceTime)
        {
            var entry = ActionHistory.GetClosestEntry(entries, referenceTime, next: false);
            if (entry == null)
                return -1;
            else
                return referenceTime - entry.GetTime();
        }

        public double SelectNext(double referenceTime)
        {
            if (lastFocusedWindow != EditorWindow.focusedWindow)
            {
                lastFocusedWindow = EditorWindow.focusedWindow;
            }

            var entry = ActionHistory.GetClosestEntry(entries, referenceTime, next: true);
            if (entry != null)
            {
                select((Entry)entry);
                return entry.GetTime();
            }
            else
                return -1;
        }

        public double SelectPrevious(double referenceTime)
        {
            if (lastFocusedWindow != EditorWindow.focusedWindow)
            {
                lastFocusedWindow = EditorWindow.focusedWindow;
            }

            var entry = ActionHistory.GetClosestEntry(entries, referenceTime, next: false);
            if (entry != null)
            {
                select((Entry)entry);
                return entry.GetTime();
            }
            else
                return -1;
        }

        protected void select(Entry entry)
        {
            if (entry.EditorWindow != null)
            {
                entry.EditorWindow.Focus();
                lastFocusedWindow = entry.EditorWindow;
            }
            
            ignoreNextChange = true;

            _onEntryRestored?.Invoke(this, entry.GetTime());
        }

        private void addEntry(EditorWindow editorWindow)
        {
            if (ignoreNextChange)
            {
                ignoreNextChange = false;
                return;
            }

            if (entries.Count >= maxEntries)
            {
                entries.RemoveAt(0);
            }

            double time = EditorApplication.timeSinceStartup;
            var newEntry = new Entry(time, editorWindow);
            entries.Add(newEntry);

            _onNewEntryAdded?.Invoke(this, time);
        }
    }
}
#endif
