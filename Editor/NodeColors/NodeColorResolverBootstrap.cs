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
            var settings = NodeColorSettings.instance;

            if (NodeColorSettingsUtility.TryGetMacroKey(element, out var macroKey) &&
                settings.TryGetMacroColor(macroKey, out var macroChoice))
            {
                return NodeColorSettingsUtility.ToMix(macroChoice);
            }

            var type = element?.GetType();
            if (settings.TryGetTypeColor(type, out var typeChoice))
            {
                return NodeColorSettingsUtility.ToMix(typeChoice);
            }

            return null;
        }

        private static IEnumerable<DropdownOption> ProvideContextOptions(IGraphElement element, GraphReference reference, GraphSelection selection)
        {
            return NodeColorSettingsUtility.BuildContextOptions(element, reference, selection);
        }
    }
}
