#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;

namespace Kamgam.MouseShortcuts
{
    public class MouseCommand
    {
        public string Id;
        public bool IsShortcut;
        public MethodInfo Method;

        public MouseCommand(string id, MethodInfo method, bool isShortcut = false)
        {
            Id = id;
            Method = method;
            IsShortcut = isShortcut;
        }

        public bool Invoke()
        {
            if (Method != null)
            {
                if (IsShortcut)
                {
                    if (Method.GetParameters().Length == 0)
                    {
                        Method.Invoke(null, new object[0]);
                        return true;
                    }
                    else if (Method.GetParameters()[0].ParameterType == typeof(ShortcutArguments))
                    {
                        // Does not yet work.
                        /*
                        var args = new ShortcutArguments
                        {
                            context = EditorWindow.focusedWindow,
                            stage = ShortcutStage.End
                        };
                        Method.Invoke(null, new object[] { args });
                        return true;
                        */
                    }
                }
                else
                {
                    if (Method.GetParameters().Length == 0)
                    {
                        Method.Invoke(null, new object[0]);
                        return true;
                    }
                    else if (Method.GetParameters()[0].ParameterType == typeof(MenuCommand))
                    {
                        Method.Invoke(null, new[] { new MenuCommand(null) });
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
#endif
