using Blackhole.ModuleSystem.Composition;
using Blackhole.Settings.Contract;
using UnityEngine;

namespace Blackhole.Framework.Composition
{
    [ProjectSettings("Framework", 0)]
    [CreateAssetMenu(fileName = ResourceName, menuName = "Blackhole/Framework/Project Settings")]
    public sealed class BlackholeProjectSettingsSO : ScriptableObject
    {
        public const string ResourceName = "BlackholeProjectSettings";

        [SerializeField] private ModuleManifestCatalogSO moduleCatalog;
        [SerializeField] private ModuleManifestSO startingModule;
        [SerializeField] private string frameworkScene;

        public ModuleManifestCatalogSO ModuleCatalog => moduleCatalog;
        public ModuleManifestSO StartingModule => startingModule;
        public string FrameworkScene => frameworkScene;
    }
}
