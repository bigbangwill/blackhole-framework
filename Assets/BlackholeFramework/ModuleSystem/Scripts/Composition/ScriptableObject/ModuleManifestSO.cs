using Blackhole.SharedKernel.Composition;
using UnityEngine;

namespace Blackhole.ModuleSystem.Composition
{
    [CreateAssetMenu(fileName = "ModuleManifest", menuName = "Blackhole/Modules/Module System/Module Manifest", order = 1)]
    public class ModuleManifestSO : ScriptableObject
    {
        [SerializeField] protected string moduleID;
        [SerializeField] protected ModuleInstallerSO installer;
        [SerializeField] protected string moduleScene;
        [SerializeField] protected bool isRootModule;
        [SerializeField] protected bool isPersistant;
        [Tooltip("Install a separate copy in a child scope even when an ancestor already provides this module. Dependencies follow their own settings.")]
        [SerializeField] protected bool allowChildScopeOverride;
        [SerializeField] protected ModuleManifestSO[] dependencies;

        public virtual string ModuleID => moduleID;
        public virtual ModuleInstallerSO Installer => installer;
        public virtual string ModuleScene => moduleScene;
        public virtual bool IsRootModule => isRootModule;
        public virtual bool IsPersistant => isPersistant;
        public virtual bool AllowChildScopeOverride => allowChildScopeOverride;
        public virtual ModuleManifestSO[] Dependencies => dependencies;
    }
}
