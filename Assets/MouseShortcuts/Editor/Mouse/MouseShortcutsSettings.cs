using Kamgam.NativeMouseHookLib;
using UnityEditor;
using UnityEngine;

namespace Kamgam.MouseShortcuts
{
    public enum LogLevel
    {
        Log = 0,
        Warning = 1,
        Error = 2,
        NoLogs = 99
    }

    public class MouseShortcutsSettings : ScriptableObject
    {
        public const string Version = "1.2.0";
        public const string SettingsFilePath = "Assets/MouseShortcutsSettings.asset";

        [SerializeField, Tooltip(_LogLevelTooltip)]
        public LogLevel LogLevel;
        public const string _LogLevelTooltip = "Log levels to determine how many log messages will be shown (Log = all message, Error = only critical errors).";

        [SerializeField, Tooltip(_TrackWindowsTooltip)]
        public bool TrackWindows;
        public const string _TrackWindowsTooltip = "Should focus changes of editor windows be added to the history?";

        public MouseShortcutsData Data;

        protected static MouseShortcutsSettings cachedSettings;

        public static MouseShortcutsSettings GetOrCreateSettings()
        {
            if (cachedSettings == null)
            {
                cachedSettings = AssetDatabase.LoadAssetAtPath<MouseShortcutsSettings>(SettingsFilePath);
                if (cachedSettings == null)
                {
                    cachedSettings = ScriptableObject.CreateInstance<MouseShortcutsSettings>();

                    cachedSettings.LogLevel = LogLevel.Warning;
                    cachedSettings.TrackWindows = false;

                    if (cachedSettings.Data == null)
                        cachedSettings.Data = new MouseShortcutsData();
                    cachedSettings.Data.CreateOrFillData();

                    AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);
                    AssetDatabase.SaveAssets();

                    // check if mouse events are supported
                    if (!NativeMouseHook.IsSupported)
                    {
                        EditorUtility.DisplayDialog("Mouse Shortcuts not supported.", "Native mouse events are not supported on your current platform. Therefore this tool can not be used.", "Understood");
                    }

                    if (!MouseShortcutsWindow.IsOpen())
                    {
                        bool openWindow = EditorUtility.DisplayDialog("Mouse Shortcuts", "Welcome to Mouse Shortcuts!\nYou can always open the shortcut window through the menu: Tools > Mouse Shortcuts > Open Window\n\nWould you like to open the window now?", "Yes (open shortcuts window)", "No");
                        if (openWindow)
                        {
                            MouseShortcutsWindow.GetOrOpen();
                        }
                    }
                }
            }
            return cachedSettings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        // settings
        public static void SelectSettings()
        {
            var settings = MouseShortcutsSettings.GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "MouseShortcutsSettings settings could not be found or created.", "Ok");
            }
        }

        [MenuItem("Tools/Mouse Shortcuts/Open Settings", priority = 200)]
        public static void OpenSettings()
        {
            var settings = GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Settings could not be found or created.", "Ok");
            }
        }

        [MenuItem("Tools/Mouse Shortcuts/Open Manual", priority = 201)]
        public static void OpenManual()
        {
            EditorUtility.OpenWithDefaultApp("Assets/MouseShortcuts/MouseShortcutsManual.pdf");
        }

        [MenuItem("Tools/Mouse Shortcuts/Please leave a review :-)", priority = 500)]
        public static void Review()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/228013");
        }

        [MenuItem("Tools/Mouse Shortcuts/More Assets by KAMGAM", priority = 501)]
        public static void MoreAssets()
        {
            Application.OpenURL("https://kamgam.com/unity");
        }

        [MenuItem("Tools/Mouse Shortcuts/Version " + Version, priority = 502)]
        public static void LogVersion()
        {
            Debug.Log("Mouse Shortcuts - version " + Version + ", Unity: " + Application.unityVersion);
        }
    }

    static class PivotCursorToolSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreatePivotCursorToolSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Mouse Button Shortcuts", SettingsScope.Project)
            {
                label = "Mouse Shortctus",
                guiHandler = (searchContext) =>
                {
                    var serializedSettings = MouseShortcutsSettings.GetSerializedSettings();
                    var settings = serializedSettings.targetObject as MouseShortcutsSettings;

                    beginHorizontalIndent(10);

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Version: " + MouseShortcutsSettings.Version + "  (" + MouseShortcutsSettings.SettingsFilePath + ")");
                    if (drawButton(" Manual ", icon: "_Help", options: GUILayout.Width(80)))
                    {
                        MouseShortcutsSettings.OpenManual();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                    drawField(serializedSettings, "LogLevel", "Log level:", MouseShortcutsSettings._LogLevelTooltip);
                    drawField(serializedSettings, "TrackWindows", "Track Windows:", MouseShortcutsSettings._TrackWindowsTooltip);

                    endHorizontalIndent();

                    serializedSettings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting.
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "ui", "preview", "canvas", "editor", "inspector", "ui preview" })
            };

            return provider;
        }

        static bool drawButton(string text, string tooltip = null, string icon = null, params GUILayoutOption[] options)
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

        static void drawField(SerializedObject settings, string fieldPropertyName, string label, string tooltip = null)
        {
            EditorGUILayout.PropertyField(settings.FindProperty(fieldPropertyName), new GUIContent(label));
            if (!string.IsNullOrEmpty(tooltip))
            {
                var style = new GUIStyle(GUI.skin.label);
                style.wordWrap = true;
                var col = style.normal.textColor;
                col.a = 0.5f;
                style.normal.textColor = col;

                beginHorizontalIndent(10);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(tooltip, style);
                GUILayout.EndVertical();
                endHorizontalIndent();
            }
            GUILayout.Space(5);
        }

        static void beginHorizontalIndent(int indentAmount = 10, bool beginVerticalInside = true)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(indentAmount);
            if (beginVerticalInside)
                GUILayout.BeginVertical();
        }

        static void endHorizontalIndent(float indentAmount = 10, bool begunVerticalInside = true, bool bothSides = false)
        {
            if (begunVerticalInside)
                GUILayout.EndVertical();
            if (bothSides)
                GUILayout.Space(indentAmount);
            GUILayout.EndHorizontal();
        }
    }
}