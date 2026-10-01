using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Blackhole.Framework.Composition.Editor
{
    /// <summary>Discovers new manifests without generating copies of module configuration.</summary>
    [InitializeOnLoad]
    internal sealed class ModuleCatalogUpdater : AssetPostprocessor
    {
        private static bool queued;

        static ModuleCatalogUpdater() => Queue();

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom, bool didDomainReload)
        {
            if (didDomainReload || imported.Concat(deleted).Concat(moved).Concat(movedFrom).Any(path => path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)))
                Queue();
        }

        private static void Queue()
        {
            if (queued) return;
            queued = true;
            EditorApplication.delayCall += Refresh;
        }

        private static void Refresh()
        {
            queued = false;
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                Queue();
                return;
            }
            try
            {
                FrameworkProjectSetup.RegenerateSettings();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Blackhole configuration could not refresh: {exception.Message}");
            }
        }
    }
}
