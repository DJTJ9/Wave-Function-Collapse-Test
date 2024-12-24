#if UNITY_EDITOR
using UnityEngine;

namespace Kamgam.MouseShortcuts
{
    public static class CustomCommands
    {
        // -- Do not change these -------------------

        [MouseCommandAttribute("Selection/Previous")]
        public static void SelectionPrevious()
        {
            ActionHistory.Instance.Previous();
        }

        [MouseCommandAttribute("Selection/Next")]
        public static void ShortcutSelectionNext()
        {
            ActionHistory.Instance.Next();
        }

        // -- Add your custom commands below. Add the "MouseCommandAttribute" attribute to make them show up in the list ---------

    }
}
#endif
