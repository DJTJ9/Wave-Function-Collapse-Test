#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.MouseShortcuts
{
    public class MouseShortcutsWindow : EditorWindow
    {
        public static MouseShortcutsWindow GetOrOpen()
        {
            if (!HasOpenInstances<MouseShortcutsWindow>())
            {
                var window = openWindow();
                window.Focus();
                return window;
            }
            else
            {
                var window = GetWindow<MouseShortcutsWindow>();
                window.Focus();
                return window;
            }
        }

        public static bool IsOpen()
        {
            return HasOpenInstances<MouseShortcutsWindow>();
        }

        [MenuItem("Tools/Mouse Shortcuts/Open Window", priority = 10)]
        [MenuItem("Window/Mouse Shortcuts")]
        [MenuItem("Edit/Mouse Shortcuts..", priority = 262)]
        static MouseShortcutsWindow openWindow()
        {
            MouseShortcutsWindow window = (MouseShortcutsWindow)EditorWindow.GetWindow(typeof(MouseShortcutsWindow));
            window.titleContent = new GUIContent("Mouse Button Shortcuts");
            window.Initialize();
            window.Show();
            return window;
        }

        public void OnEnable()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (!isDocked())
            {
                if (position.width < 435 || position.height < 150)
                {
                    const int width = 435;
                    const int height = 150;
                    var x = Screen.currentResolution.width / 2 - width;
                    var y = Screen.currentResolution.height / 2 - height;
                    position = new Rect(x, y, width, height);
                }
            }

            ClearShortcutIds();
        }

        private List<string> _cachedShortcutIds = new List<string>();

        public List<string> GetShortcutIds()
        {
            if (_cachedShortcutIds.Count == 0) 
            {
                var commands = MouseShortcuts.Instance.Commands.Commands;
                foreach (var command in commands)
                {
                    _cachedShortcutIds.Add(command.Id);
                }
            }

            return _cachedShortcutIds;
        }

        public void ClearShortcutIds()
        {
            _cachedShortcutIds.Clear();
        }

        protected bool isDocked()
        {
#if UNITY_2020_1_OR_NEWER
            return docked;
#else
            return true;
#endif
        }

        void OnGUI()
        {
            try
            {
                var settings = MouseShortcutsSettings.GetOrCreateSettings();

                // Top Button bar
                {
                    GUILayout.BeginHorizontal();
                    DrawLabel("Shortcuts", bold: true, options: GUILayout.MinWidth(150));

                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Version " + MouseShortcutsSettings.Version + " ");
                    if (DrawButton(" Manual ", icon: "_Help"))
                    {
                        OpenManual();
                    }
                    if (DrawButton(" Settings ", icon: "_Popup"))
                    {
                        OpenSettings();
                    }
                    if (DrawButton(" C# "))
                    {
                        OpenCustomCommandFile();
                    }

                    GUILayout.EndHorizontal();
                }

                DrawLabel("Use the dropdowns to assign commands to each mouse button.", wordwrap: true);
                GUILayout.BeginVertical(EditorStyles.helpBox);

                var shortcutIds = GetShortcutIds();
                foreach (var mouseShortcut in settings.Data.Shortcuts)
                {
                    string id = DrawCommandsPopup(mouseShortcut.CommandId, mouseShortcut.MouseEvent.ToString(), shortcutIds);
                    if(!string.IsNullOrEmpty(id))
                    {
                        mouseShortcut.CommandId = id;
                    }
                }

                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }
            catch
            {
                throw;
            }
        }

        private string DrawCommandsPopup(string currentShortcutId, string text, List<string> shortcutIds)
        {
            int selectedIndex = 0;

            var options = new GUIContent[shortcutIds.Count];
            for (int i = 0; i < shortcutIds.Count; i++)
            {
                if (shortcutIds[i] == currentShortcutId)
                    selectedIndex = i;
                string label = shortcutIds[i];
                if (shortcutIds[i] == MouseCommands.CommandIdDoNothing)
                    label = "-";
                options[i] = new GUIContent(label);
            }

            if (selectedIndex < 0)
                return currentShortcutId;

            selectedIndex = EditorGUILayout.Popup(new GUIContent(text), selectedIndex, options);
            if (shortcutIds.Count == 0)
                return null;
            else
                return shortcutIds[selectedIndex];
        }

        public static void OpenManual()
        {
            EditorUtility.OpenWithDefaultApp("Assets/MouseShortcuts/MouseShortcutsManual.pdf");
        }

        public void OpenSettings()
        {
            MouseShortcutsSettings.SelectSettings();
        }

        public static void OpenCustomCommandFile()
        {
            // OPens in Unity configured text editor.
            var obj = AssetDatabase.LoadAssetAtPath<Object>("Assets/MouseShortcuts/Editor/CustomCommands.cs");
            AssetDatabase.OpenAsset(obj.GetInstanceID());

            // Opens in system default text edtior.
            // EditorUtility.OpenWithDefaultApp("Assets/MouseShortcuts/Editor/CustomCommands.cs");
        }

        public static void DrawLabel(string text, Color? color = null, bool bold = false, bool wordwrap = true, bool richText = true, Texture icon = null, params GUILayoutOption[] options)
        {
            if (!color.HasValue)
                color = GUI.skin.label.normal.textColor;

            var style = new GUIStyle(GUI.skin.label);
            if (bold)
                style.fontStyle = FontStyle.Bold;

            style.normal.textColor = color.Value;
            style.wordWrap = wordwrap;
            style.richText = richText;
            style.imagePosition = ImagePosition.ImageLeft;

            var content = new GUIContent();
            content.text = text;
            if (icon != null)
            {
                GUILayout.Space(16);
                var position = GUILayoutUtility.GetRect(content, style);
                GUI.DrawTexture(new Rect(position.x - 16, position.y, 16, 16), icon);
                GUI.Label(position, content, style);
            }
            else
            {
                GUILayout.Label(text, style, options);
            }
        }

        public static bool DrawButton(string text, string tooltip = null, string icon = null, params GUILayoutOption[] options)
        {
            GUIContent content;

            // icon
            if (!string.IsNullOrEmpty(icon))
                content = EditorGUIUtility.IconContent(icon);
            else
                content = new GUIContent();

            // text
            content.text = text;

            // tooltip
            if (!string.IsNullOrEmpty(tooltip))
                content.tooltip = tooltip;

            return GUILayout.Button(content, options);
        }
    }
}
#endif
