using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace CHM.VisualScriptingKai.Editor
{
    public enum NodeColorChoice
    {
        Gray = (int)NodeColor.Gray,
        Blue = (int)NodeColor.Blue,
        Teal = (int)NodeColor.Teal,
        Green = (int)NodeColor.Green,
        Yellow = (int)NodeColor.Yellow,
        Orange = (int)NodeColor.Orange,
        Red = (int)NodeColor.Red,
        TealReadable = 100,
        Purple = 200,
        Pink = 201,
        Amber = 202,
        Mint = 203
    }

    [Serializable]
    [FilePath("ProjectSettings/VisualScriptingKaiNodeColors.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class NodeColorSettings : ScriptableSingleton<NodeColorSettings>
    {
        [Serializable]
        private struct TypeEntry
        {
            public string typeName;
            public NodeColorChoice color;
        }

        [Serializable]
        private struct MacroEntry
        {
            public string macroKey;
            public NodeColorChoice color;
        }

        [SerializeField]
        private List<TypeEntry> typeEntries = new List<TypeEntry>();

        [SerializeField]
        private List<MacroEntry> macroEntries = new List<MacroEntry>();

        private Dictionary<string, NodeColorChoice> typeCache;
        private Dictionary<string, NodeColorChoice> macroCache;

        private void OnEnable()
        {
            if (typeEntries == null)
            {
                typeEntries = new List<TypeEntry>();
            }

            if (macroEntries == null)
            {
                macroEntries = new List<MacroEntry>();
            }

            RebuildCaches();
        }

        private void RebuildCaches()
        {
            typeCache = new Dictionary<string, NodeColorChoice>();
            macroCache = new Dictionary<string, NodeColorChoice>();

            foreach (var entry in typeEntries)
            {
                if (!string.IsNullOrEmpty(entry.typeName))
                {
                    typeCache[entry.typeName] = entry.color;
                }
            }

            foreach (var entry in macroEntries)
            {
                if (!string.IsNullOrEmpty(entry.macroKey))
                {
                    macroCache[entry.macroKey] = entry.color;
                }
            }
        }

        private void EnsureCaches()
        {
            if (typeCache == null || macroCache == null)
            {
                RebuildCaches();
            }
        }

        private static string Normalize(Type type)
        {
            return type?.AssemblyQualifiedName;
        }

        public bool TryGetTypeColor(Type type, out NodeColorChoice choice)
        {
            EnsureCaches();
            choice = default;

            var key = Normalize(type);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            return typeCache.TryGetValue(key, out choice);
        }

        public bool TryGetMacroColor(string macroKey, out NodeColorChoice choice)
        {
            EnsureCaches();
            choice = default;

            if (string.IsNullOrEmpty(macroKey))
            {
                return false;
            }

            return macroCache.TryGetValue(macroKey, out choice);
        }

        public void SetTypeColor(Type type, NodeColorChoice choice)
        {
            var key = Normalize(type);
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            EnsureCaches();

            typeCache[key] = choice;

            var entry = new TypeEntry { typeName = key, color = choice };
            var index = typeEntries.FindIndex(e => e.typeName == key);
            if (index >= 0)
            {
                typeEntries[index] = entry;
            }
            else
            {
                typeEntries.Add(entry);
            }

            Save(true);
        }

        public void SetMacroColor(string macroKey, NodeColorChoice choice)
        {
            if (string.IsNullOrEmpty(macroKey))
            {
                return;
            }

            EnsureCaches();

            macroCache[macroKey] = choice;

            var entry = new MacroEntry { macroKey = macroKey, color = choice };
            var index = macroEntries.FindIndex(e => e.macroKey == macroKey);
            if (index >= 0)
            {
                macroEntries[index] = entry;
            }
            else
            {
                macroEntries.Add(entry);
            }

            Save(true);
        }

        public void RemoveTypeColor(Type type)
        {
            var key = Normalize(type);
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            EnsureCaches();

            if (!typeCache.Remove(key))
            {
                return;
            }

            for (var i = typeEntries.Count - 1; i >= 0; --i)
            {
                if (typeEntries[i].typeName == key)
                {
                    typeEntries.RemoveAt(i);
                    break;
                }
            }

            Save(true);
        }

        public void RemoveMacroColor(string macroKey)
        {
            if (string.IsNullOrEmpty(macroKey))
            {
                return;
            }

            EnsureCaches();

            if (!macroCache.Remove(macroKey))
            {
                return;
            }

            for (var i = macroEntries.Count - 1; i >= 0; --i)
            {
                if (macroEntries[i].macroKey == macroKey)
                {
                    macroEntries.RemoveAt(i);
                    break;
                }
            }

            Save(true);
        }

        public void ClearAll()
        {
            typeEntries.Clear();
            macroEntries.Clear();
            typeCache?.Clear();
            macroCache?.Clear();
            Save(true);
        }
    }
}
