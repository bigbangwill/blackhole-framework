using Blackhole.SharedKernel.Composition;
using UnityEngine;

namespace Blackhole.ModuleSystem.Composition
{
    [CreateAssetMenu(fileName = "ModuleManifest", menuName = "Blackhole/Modules/Module System/Module Manifest", order = 1)]
    public class ModuleManifestSO : ScriptableObject
    {
        [SerializeField] private string moduleID;
        [SerializeField] private ModuleInstallerSO installer;
        [SerializeField] private string moduleScene;
        [SerializeField] private bool isPersistant;
        [Tooltip("Install a separate copy in a child scope even when an ancestor already provides this module. Dependencies follow their own settings.")]
        [SerializeField] private bool allowChildScopeOverride;
        [SerializeField] private ModuleManifestSO[] dependencies;

        public string ModuleID => moduleID;
        public ModuleInstallerSO Installer => installer;
        public string ModuleScene => moduleScene;
        public bool IsPersistant => isPersistant;
        public bool AllowChildScopeOverride => allowChildScopeOverride;
        public ModuleManifestSO[] Dependencies => dependencies;
    }
}
