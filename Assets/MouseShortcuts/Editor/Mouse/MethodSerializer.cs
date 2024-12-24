#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEditor.ShortcutManagement;

namespace Kamgam.MouseShortcuts
{
    public static class MethodSerializer
    {
        public static byte[] SerializeMethodInfo(MethodInfo methodInfo)
        {
            var memoryStream = new MemoryStream();
            using (memoryStream)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, methodInfo);
            }
            return memoryStream.ToArray();
        }

        public static MethodInfo DeserializeMethodInfo(byte[] serializedData)
        {
            var memoryStream = new MemoryStream(serializedData);
            using (memoryStream)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (MethodInfo)formatter.Deserialize(memoryStream);
            }
        }
    }
}
#endif
