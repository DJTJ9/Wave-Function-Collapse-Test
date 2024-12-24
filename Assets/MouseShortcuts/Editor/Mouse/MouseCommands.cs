#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Unity.Jobs;

namespace Kamgam.MouseShortcuts
{
    public class MouseCommands
    {
        // Used for the default commands in MouseShortcutsData
        public const string CommandIdDoNothing = "DoNothing";
        public const string CommandIdSelectionPrevious = "Selection/Previous";
        public const string CommandIdSelectionNext = "Selection/Next";
        
        public List<MouseCommand> Commands;

        protected JobHandle _jobHandle;

        // Buffers for multithreading
        public NativeArray<byte> Buffer = new NativeArray<byte>(15_000, Allocator.Persistent);
        public NativeArray<MouseCommandSerialized> SerializedCommands = new NativeArray<MouseCommandSerialized>(500, Allocator.Persistent);
        public NativeArray<int> SerializedCommandsLength = new NativeArray<int>(1, Allocator.Persistent);

        public MouseCommands() 
        {
            Commands = new List<MouseCommand>();

            // Multithreaded command discovery (because it may take a few seconds).
            _jobHandle = new MouseCommandsDiscoveryJob()
            {
                Buffer = Buffer,
                SerializedCommands = SerializedCommands,
                SerializedCommandsLength = SerializedCommandsLength
            }.Schedule();
            EditorApplication.update += waitForCommandsJob;

            // The old way without multithreading
            /*
            addMouseCommands(Commands);
            addMenuItems(Commands);
            addShortcuts(Commands);
            Commands.Sort((a, b) => string.Compare(a.Id, b.Id));
            Commands.Insert(0, new MouseCommand(CommandIdDoNothing, null));
            */
        }

        private void waitForCommandsJob()
        {
            if (!_jobHandle.IsCompleted)
                return;

            _jobHandle.Complete();
            EditorApplication.update -= waitForCommandsJob;

            Commands.Clear();
            int numOfCommands = SerializedCommandsLength[0];
            for (int i = 0; i < numOfCommands; i++)
            {
                var command = SerializedCommands[i].Deserialize(Buffer);
                Commands.Add(command);
            }
            Dispose();

            Commands.Sort((a, b) => string.Compare(a.Id, b.Id));
            Commands.Insert(0, new MouseCommand(CommandIdDoNothing, null));
        }

        private void Dispose() 
        {
            SerializedCommandsLength.Dispose();
            SerializedCommands.Dispose();
            Buffer.Dispose();
        }

        ~MouseCommands()
        {
            Dispose();
        }

        public bool Invoke(string commandId)
        {
            foreach (var cmd in Commands)
                if (cmd.Id == commandId)
                    return cmd.Invoke();

            return false;
        }

        // Old non-multithreaded command discovery
        /*
        protected void addMouseCommands(List<MouseCommand> commands) 
        {
            try
            {
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
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
                            var command = new MouseCommand(name, method);
                            commands.Add(command);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                logInnerReflectionErrors(ex);
            }
        }

        static void logInnerReflectionErrors(ReflectionTypeLoadException ex)
        {
            foreach (Exception inner in ex.LoaderExceptions)
            {
                Debug.LogWarning("MouseCommand Code Discovery Error: " + inner.Message);
            }
        }

        protected void addMenuItems(List<MouseCommand> commands)
        {
            try
            {
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
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
                                var command = new MouseCommand(name, method);
                                commands.Add(command);
                            }
                        }
                    }
                }
            }
            catch { };
        }

        protected void addShortcuts(List<MouseCommand> commands)
        {
            try
            {
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
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
                                var command = new MouseCommand(name, method, true);
                                commands.Add(command);
                            }
                        }
                    }
                }
            }
            catch { };
        }
        */
    }
}
#endif
