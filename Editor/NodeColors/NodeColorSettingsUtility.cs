using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CHM.VisualScriptingKai.Editor
{
    internal static class NodeColorSettingsUtility
    {
        private static readonly (NodeColorChoice choice, string label)[] Palette =
        {
            (NodeColorChoice.Gray, "Gray"),
            (NodeColorChoice.Blue, "Blue"),
            (NodeColorChoice.Teal, "Teal"),
            (NodeColorChoice.Green, "Green"),
            (NodeColorChoice.Yellow, "Yellow"),
            (NodeColorChoice.Orange, "Orange"),
            (NodeColorChoice.Red, "Red"),
            (NodeColorChoice.TealReadable, "Teal (Readable)"),
            (NodeColorChoice.Purple, "Purple"),
            (NodeColorChoice.Pink, "Pink"),
            (NodeColorChoice.Amber, "Amber"),
            (NodeColorChoice.Mint, "Mint")
        };

        internal static NodeColorMix ToMix(NodeColorChoice choice)
        {
            switch (choice)
            {
                case NodeColorChoice.TealReadable:
                    return NodeColorMix.TealReadable;
                case NodeColorChoice.Purple:
                    return new NodeColorMix { blue = 0.5f, red = 0.5f };
                case NodeColorChoice.Pink:
                    return new NodeColorMix { red = 0.7f, gray = 0.3f };
                case NodeColorChoice.Amber:
                    return new NodeColorMix { orange = 0.6f, yellow = 0.4f };
                case NodeColorChoice.Mint:
                    return new NodeColorMix { teal = 0.5f, green = 0.5f };
                default:
                    return new NodeColorMix((NodeColor)(int)choice);
            }
        }

        internal static IEnumerable<DropdownOption> BuildContextOptions(IGraphElement element, GraphReference reference, GraphSelection selection)
        {
            var targets = CollectTargets(element, selection);
            if (!targets.HasAny)
            {
                yield break;
            }

            foreach (var (choice, label) in Palette)
            {
                yield return new DropdownOption((Action)(() => ApplyColor(targets, choice)), $"Node Color/{label}");
            }

            yield return new DropdownOption((Action)(() => ApplyColor(targets, null)), "Node Color/Reset");
        }

        private static TargetBuckets CollectTargets(IGraphElement element, GraphSelection selection)
        {
            var buckets = new TargetBuckets();

            void AddElement(IGraphElement candidate)
            {
                if (candidate == null)
                {
                    return;
                }

                if (TryGetMacroKey(candidate, out var macroKey))
                {
                    buckets.Macros.Add(macroKey);
                }
                else if (IsSupportedType(candidate))
                {
                    buckets.Types.Add(candidate.GetType());
                }
            }

            if (selection != null)
            {
                foreach (var selected in selection)
                {
                    AddElement(selected);
                }
            }

            if (!buckets.HasAny)
            {
                AddElement(element);
            }

            return buckets;
        }

        private static bool IsSupportedType(object element)
        {
            return element is IUnit || element is IState || element is IStateTransition;
        }

        internal static bool TryGetMacroKey(IGraphElement element, out string key)
        {
            key = null;

            if (element is INesterUnit nesterUnit)
            {
                var macro = nesterUnit.nest?.macro as UnityEngine.Object;
                if (macro == null)
                {
                    return false;
                }

                var path = AssetDatabase.GetAssetPath(macro);
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }

                var guid = AssetDatabase.AssetPathToGUID(path);
                key = string.IsNullOrEmpty(guid) ? path : guid;
                return true;
            }

            return false;
        }

        private static void ApplyColor(TargetBuckets targets, NodeColorChoice? choice)
        {
            var settings = NodeColorSettings.instance;
            var changed = false;

            if (choice.HasValue)
            {
                foreach (var type in targets.Types)
                {
                    settings.SetTypeColor(type, choice.Value);
                    changed = true;
                }

                foreach (var macroKey in targets.Macros)
                {
                    settings.SetMacroColor(macroKey, choice.Value);
                    changed = true;
                }
            }
            else
            {
                foreach (var type in targets.Types)
                {
                    settings.RemoveTypeColor(type);
                    changed = true;
                }

                foreach (var macroKey in targets.Macros)
                {
                    settings.RemoveMacroColor(macroKey);
                    changed = true;
                }
            }

            if (!changed)
            {
                return;
            }

            GraphWindow.active?.context?.canvas?.Recollect();
            GraphWindow.active?.Repaint();
            InternalEditorUtility.RepaintAllViews();
        }

        private class TargetBuckets
        {
            public TargetBuckets()
            {
                Types = new HashSet<Type>();
                Macros = new HashSet<string>();
            }

            public HashSet<Type> Types { get; }
            public HashSet<string> Macros { get; }

            public bool HasAny => Types.Count > 0 || Macros.Count > 0;
        }
    }
}
