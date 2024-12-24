#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Jobs;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Unity.Collections;

namespace Kamgam.MouseShortcuts
{
    public struct MouseCommandsDiscoveryJob : IJob
    {
        [WriteOnly] public NativeArray<byte> Buffer;
        [WriteOnly] public NativeArray<MouseCommandSerialized> SerializedCommands;
        [WriteOnly] public NativeArray<int> SerializedCommandsLength;

        public void Execute()
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            int bufferIndex = 0;
            int commandsIndex = 0;
            commandsIndex = addMouseCommands(assemblies, Buffer, ref bufferIndex, SerializedCommands, commandsIndex);
            commandsIndex = addMenuItems(assemblies, Buffer, ref bufferIndex, SerializedCommands, commandsIndex);
            SerializedCommandsLength[0] = addShortcuts(assemblies, Buffer, ref bufferIndex, SerializedCommands, commandsIndex);
        }

        int addMouseCommands(Assembly[] assemblies, NativeArray<byte> buffer, ref int bufferIndex, NativeArray<MouseCommandSerialized> commands, int index)
        {
            try
            {
                foreach (var assembly in assemblies)
                {
                    MethodInfo[] methods = null;
                    try
                    {
                        methods = assembly.GetTypes()
                          .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                          .Where(m => m.GetCustomAttributes(typeof(MouseCommandAttribute), false).Length > 0)
                          .ToArray();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        logInnerReflectionErrors(e);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("MouseCommand Code Discovery Error: " + e.Message);
                        continue;
                    }

                    foreach (var method in methods)
                    {
                        var attributes = method.CustomAttributes;
                        var attribute = attributes.First();
                        if (attribute.ConstructorArguments.Count == 1)
                        {
                            string name = attribute.ConstructorArguments[0].Value.ToString();

                            // Debug.Log("Mouse Command: " + name);
                            var command = new MouseCommandSerialized(buffer, ref bufferIndex, name, method);
                            commands[index] = command;
                            index++;
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                logInnerReflectionErrors(ex);
            }

            return index;
        }

        static void logInnerReflectionErrors(ReflectionTypeLoadException ex)
        {
            foreach (Exception inner in ex.LoaderExceptions)
            {
                Debug.LogWarning("MouseCommand Code Discovery Error: " + inner.Message);
            }
        }

        int addMenuItems(Assembly[] assemblies, NativeArray<byte> buffer, ref int bufferIndex, NativeArray<MouseCommandSerialized> commands, int index)
        {
            try
            {
                foreach (var assembly in assemblies)
                {
                    var methods = assembly.GetTypes()
                          .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                          .Where(m => m.GetCustomAttributes(typeof(MenuItem), false).Length > 0)
                          .ToArray();
                    foreach (var method in methods)
                    {
                        var attributes = method.CustomAttributes;
                        var attribute = attributes.First();
                        if (attribute.ConstructorArguments.Count == 1)
                        {
                            string name = attribute.ConstructorArguments[0].Value.ToString();
                            if (!name.StartsWith("CONTEXT") && !name.StartsWith("internal:"))
                            {
                                // Debug.Log("MenuItem: " + name);
                                var command = new MouseCommandSerialized(buffer, ref bufferIndex, name, method);
                                commands[index] = command;
                                index++;
                            }
                        }
                    }
                }
            }
            catch { }

            return index;
        }

        int addShortcuts(Assembly[] assemblies, NativeArray<byte> buffer, ref int bufferIndex, NativeArray<MouseCommandSerialized> commands, int index)
        {
            try
            {
                foreach (var assembly in assemblies)
                {
                    // TODO: What about ClutchShortcutAttribute?
                    var methods = assembly.GetTypes()
                          .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                          .Where(m => m.GetCustomAttributes(typeof(ShortcutAttribute), false).Length > 0)
                          .ToArray();
                    foreach (var method in methods)
                    {
                        // TODO: What about shortcuts with ShortcutArguments? We do ignore them at the moment.
                        if (method.GetParameters().Length == 0)
                        {
                            var attributes = method.CustomAttributes;
                            var attribute = attributes.First();
                            if (attribute.ConstructorArguments.Count >= 1)
                            {
                                string name = attribute.ConstructorArguments[0].Value.ToString();
                                var command = new MouseCommandSerialized(buffer, ref bufferIndex, name, method, isShortcut: true);
                                commands[index] = command;
                                index++;
                            }
                        }
                    }
                }
            }
            catch { };

            return index;
        }
    }
}
#endif
