#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Text;
using Unity.Collections;
using UnityEditor;
using UnityEditor.ShortcutManagement;

namespace Kamgam.MouseShortcuts
{
    public struct MouseCommandSerialized
    {
        // Since we can have neither managed nor unmanaged collections here
        // we use an external buffer and only reference the indices.

        public int IdBufferIndex;
        public int IdBufferLength;

        public int MethodBufferIndex;
        public int MethodBufferLength;

        public bool IsShortcut;

        public MouseCommandSerialized(NativeArray<byte> buffer, ref int bufferIndex, string id, MethodInfo method, bool isShortcut = false)
        {
            var idBytes = Encoding.UTF8.GetBytes(id);
            IdBufferIndex = bufferIndex;
            IdBufferLength = idBytes.Length;
            for (int i = 0; i < idBytes.Length; i++)
            {
                buffer[IdBufferIndex + i] = idBytes[i];
            }

            var methodBytes = MethodSerializer.SerializeMethodInfo(method);
            MethodBufferIndex = bufferIndex + idBytes.Length;
            MethodBufferLength = methodBytes.Length;
            for (int i = 0; i < methodBytes.Length; i++)
            {
                buffer[MethodBufferIndex + i] = methodBytes[i];
            }

            IsShortcut = isShortcut;

            bufferIndex = MethodBufferIndex + MethodBufferLength;
        }

        public MouseCommand Deserialize(NativeArray<byte> buffer) 
        {
            var idBytes = new byte[IdBufferLength];
            for (int i = 0; i < IdBufferLength; i++)
            {
                idBytes[i] = buffer[IdBufferIndex + i];
            }
            var id = Encoding.UTF8.GetString(idBytes);

            var methodBytes = new byte[MethodBufferLength];
            for (int i = 0; i < MethodBufferLength; i++)
            {
                methodBytes[i] = buffer[MethodBufferIndex + i];
            }
            var method = MethodSerializer.DeserializeMethodInfo(methodBytes);
            
            return new MouseCommand(id, method, IsShortcut);
        }
    }
}
#endif
