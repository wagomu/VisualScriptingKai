using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;

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
            var types = CollectTargetTypes(element, selection);
            if (types.Count == 0)
            {
                yield break;
            }

            foreach (var (choice, label) in Palette)
            {
                yield return new DropdownOption((Action)(() => ApplyColor(types, choice)), $"Node Color/{label}");
            }

            yield return new DropdownOption((Action)(() => ApplyColor(types, null)), "Node Color/Reset");
        }

        private static List<Type> CollectTargetTypes(IGraphElement element, GraphSelection selection)
        {
            var types = new List<Type>();

            if (selection != null)
            {
                foreach (var selected in selection)
                {
                    if (IsSupported(selected))
                    {
                        types.Add(selected.GetType());
                    }
                }
            }

            if (types.Count == 0 && IsSupported(element))
            {
                types.Add(element.GetType());
            }

            return types.Distinct().ToList();
        }

        private static bool IsSupported(object element)
        {
            return element is IUnit || element is IState || element is IStateTransition;
        }

        private static void ApplyColor(IEnumerable<Type> types, NodeColorChoice? choice)
        {
            var settings = NodeColorSettings.instance;
            var changed = false;

            foreach (var type in types)
            {
                if (choice.HasValue)
                {
                    settings.SetColor(type, choice.Value);
                }
                else
                {
                    settings.RemoveColor(type);
                }

                changed = true;
            }

            if (!changed)
            {
                return;
            }

            GraphWindow.active?.context?.canvas?.Recollect();
            GraphWindow.active?.Repaint();
            InternalEditorUtility.RepaintAllViews();
        }
    }
}
