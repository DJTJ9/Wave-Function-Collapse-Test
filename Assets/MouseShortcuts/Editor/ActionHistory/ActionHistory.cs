#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kamgam.MouseShortcuts
{
    /// <summary>
    /// Parent of all *History objects. Coordinates and creates them.
    /// </summary>
    public class ActionHistory
    {
        #region singleton
        private static ActionHistory _instance;

        public static ActionHistory Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ActionHistory();

                return _instance;
            }
        }
        #endregion

        protected List<IHistory> histories;
        protected double timeInHistory;
        const double TIME_EPSILON = 0.01;

        protected SelectionHistory selectionHistory;
        protected EditorWindowHistory editorWindowHistory;

        ActionHistory()
        {
            histories = new List<IHistory>();

            selectionHistory = new SelectionHistory();
            histories.Add(selectionHistory);

            editorWindowHistory = new EditorWindowHistory();
            histories.Add(editorWindowHistory);
        }

        public void Init()
        {
            foreach (var history in histories)
            {
                history.Init(onNewEntryAdded, onEntryRestored);
            }
        }

        ~ActionHistory()
        {
            foreach (var history in histories)
            {
                history.DeInit();
            }
        }

        protected void onNewEntryAdded(IHistory history, double time)
        {
            timeInHistory = time;

            if (history == selectionHistory)
                editorWindowHistory.RemoveRecentlyAddedEntries(ageLimitInSec: 0.3);
        }

        protected void onEntryRestored(IHistory history, double time)
        {
            if (history == selectionHistory)
                editorWindowHistory.UpdateLastFocusedWindow();
        }

        public void Next()
        {
            double minDelta = 999000;
            IHistory minHistory = null;
            foreach (var history in histories)
            {
                double timeDelta = history.GetNextTimeDelta(timeInHistory + TIME_EPSILON);
                if (timeDelta > 0 && timeDelta < minDelta && timeDelta > TIME_EPSILON)
                {
                    minDelta = timeDelta;
                    minHistory = history;
                }
            }

            if (minHistory != null)
                timeInHistory = minHistory.SelectNext(timeInHistory + TIME_EPSILON);
        }

        public void Previous()
        {
            double minDelta = 999000;
            IHistory minHistory = null;
            foreach (var history in histories)
            {
                double timeDelta = history.GetPreviousTimeDelta(timeInHistory - TIME_EPSILON);
                if (timeDelta > 0 && timeDelta < minDelta && timeDelta > TIME_EPSILON)
                {
                    minDelta = timeDelta;
                    minHistory = history;
                }
            }

            if (minHistory != null)
            {
                timeInHistory = minHistory.SelectPrevious(timeInHistory - TIME_EPSILON);
            }
        }

        /// <summary>
        /// Used by the concrete *History implementations to get the closest entry on the timeline in a certain direction (next/previous).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entries"></param>
        /// <param name="referenceTime"></param>
        /// <param name="next">true = next, false = previous</param>
        /// <returns></returns>
        public static IEntryWithTime GetClosestEntry<T>(IList<T> entries, double referenceTime, bool next) where T : IEntryWithTime
        {
            double minDelta = 999000;
            IEntryWithTime foundEntry = null;
            foreach (var entry in entries)
            {
                double delta;
                if(next)
                    delta = entry.GetTime() - referenceTime;
                else
                    delta = referenceTime - entry.GetTime();

                if (delta > 0 && delta < minDelta)
                {
                    minDelta = delta;
                    foundEntry = entry;
                }
            }

            return foundEntry;
        }
    }
}
#endif
