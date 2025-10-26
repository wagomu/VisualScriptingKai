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
        private struct Entry
        {
            public string typeName;
            public NodeColorChoice color;
        }

        [SerializeField]
        private List<Entry> entries = new List<Entry>();

        private Dictionary<string, NodeColorChoice> cache;

        private void OnEnable()
        {
            if (entries == null)
            {
                entries = new List<Entry>();
            }

            RebuildCache();
        }

        private void RebuildCache()
        {
            cache = new Dictionary<string, NodeColorChoice>();

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.typeName))
                {
                    continue;
                }

                cache[entry.typeName] = entry.color;
            }
        }

        private void EnsureCache()
        {
            if (cache == null)
            {
                RebuildCache();
            }
        }

        private static string Normalize(Type type)
        {
            return type?.AssemblyQualifiedName;
        }

        public bool TryGetColor(Type type, out NodeColorChoice choice)
        {
            EnsureCache();
            choice = default;

            var key = Normalize(type);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            return cache.TryGetValue(key, out choice);
        }

        public void SetColor(Type type, NodeColorChoice choice)
        {
            var key = Normalize(type);
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            EnsureCache();

            cache[key] = choice;

            var entry = new Entry { typeName = key, color = choice };
            var index = entries.FindIndex(e => e.typeName == key);
            if (index >= 0)
            {
                entries[index] = entry;
            }
            else
            {
                entries.Add(entry);
            }

            Save(true);
        }

        public void RemoveColor(Type type)
        {
            var key = Normalize(type);
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            EnsureCache();

            if (!cache.Remove(key))
            {
                return;
            }

            for (var i = entries.Count - 1; i >= 0; --i)
            {
                if (entries[i].typeName == key)
                {
                    entries.RemoveAt(i);
                    break;
                }
            }

            Save(true);
        }

        public void ClearAll()
        {
            entries.Clear();
            cache?.Clear();
            Save(true);
        }
    }
}
