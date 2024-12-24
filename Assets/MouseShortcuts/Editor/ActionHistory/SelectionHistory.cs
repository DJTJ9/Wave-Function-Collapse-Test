#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kamgam.MouseShortcuts
{
    /// <summary>
    /// This history tracks the changes in selected objects.
    /// </summary>
    public class SelectionHistory : IHistory
    {
        public class SelectionEntry : IEntryWithTime
        {
            public double Time;
            public UnityEngine.Object[] Objects;
            public EditorWindow EditorWindow;

            public SelectionEntry(double time, UnityEngine.Object[] selection, EditorWindow editorWindow)
            {
                Time = time;
                Objects = selection;
                EditorWindow = editorWindow;
            }

            public double GetTime()
            {
                return Time;
            }
        }

        protected int maxSelections;
        protected bool ignoreNextSelectionChange;
        protected List<SelectionEntry> selections;
        protected Action<IHistory, double> _onNewEntryAdded;
        protected Action<IHistory, double> _onEntryRestored;

        public void Init(Action<IHistory, double> onNewEntryAdded, Action<IHistory, double> onEntryRestored)
        {
            _onNewEntryAdded = onNewEntryAdded;
            _onEntryRestored = onEntryRestored;

            maxSelections = 20;
            selections = new List<SelectionEntry>(maxSelections);

            Selection.selectionChanged -= onSelectionChanged;
            Selection.selectionChanged += onSelectionChanged;
        }

        public void DeInit()
        {
            Selection.selectionChanged -= onSelectionChanged;
        }

        private void onSelectionChanged()
        {
            if (ignoreNextSelectionChange)
            {
                ignoreNextSelectionChange = false;
                return;
            }

            if (selections.Count >= maxSelections)
            {
                selections.RemoveAt(0);
            }

            double time = EditorApplication.timeSinceStartup;
            var newEntry = new SelectionEntry(time, Selection.objects, EditorWindow.focusedWindow); // TODO: reuse and reduce garbage
            selections.Add(newEntry);

            _onNewEntryAdded?.Invoke(this, time);
        }

        public double GetNextTimeDelta(double referenceTime)
        {
            var entry = ActionHistory.GetClosestEntry(selections, referenceTime, next: true);
            if (entry == null)
                return -1;
            else
                return entry.GetTime() - referenceTime;
        }

        public double GetPreviousTimeDelta(double referenceTime)
        {
            var entry = ActionHistory.GetClosestEntry(selections, referenceTime, next: false);
            if (entry == null)
                return -1;
            else
                return referenceTime - entry.GetTime();
        }

        public double SelectNext(double referenceTime)
        {
            var entry = ActionHistory.GetClosestEntry(selections, referenceTime, next: true);
            if (entry != null)
            {
                select((SelectionEntry) entry);
                return entry.GetTime();
            }
            else
                return -1;
        }

        public double SelectPrevious(double referenceTime)
        {
            var entry = ActionHistory.GetClosestEntry(selections, referenceTime, next: false);
            if (entry != null)
            {
                select((SelectionEntry)entry);
                return entry.GetTime();
            }
            else
                return -1;
        }

        protected void select(SelectionEntry entry)
        {
            Selection.objects = entry.Objects;
            entry.EditorWindow.Focus();

            ignoreNextSelectionChange = true;

            _onEntryRestored?.Invoke(this, entry.GetTime());
        }
    }
}
#endif
