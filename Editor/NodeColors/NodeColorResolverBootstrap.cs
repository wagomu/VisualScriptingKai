using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting;

namespace CHM.VisualScriptingKai.Editor
{
    [InitializeOnLoad]
    internal static class NodeColorResolverBootstrap
    {
        static NodeColorResolverBootstrap()
        {
            NodeColorOverrides.ResolveNodeColor = ResolveNodeColor;
            NodeColorOverrides.ProvideContextOptions = ProvideContextOptions;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private static void OnBeforeAssemblyReload()
        {
            NodeColorOverrides.ResolveNodeColor = null;
            NodeColorOverrides.ProvideContextOptions = null;
        }

        private static NodeColorMix? ResolveNodeColor(IGraphElement element, GraphReference reference)
        {
            var type = element?.GetType();
            if (type == null)
            {
                return null;
            }

            return NodeColorSettings.instance.TryGetColor(type, out var choice)
                ? NodeColorSettingsUtility.ToMix(choice)
                : (NodeColorMix?)null;
        }

        private static IEnumerable<DropdownOption> ProvideContextOptions(IGraphElement element, GraphReference reference, GraphSelection selection)
        {
            return NodeColorSettingsUtility.BuildContextOptions(element, reference, selection);
        }
    }
}
