using Blackhole.ModuleSystem.Composition;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Blackhole.Development
{
    public sealed class SmokeSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder) => builder.RegisterEntryPoint<SmokeRootStartup>();

        private void OnGUI()
        {
            GUI.Label(new Rect(24, 24, 650, 32), SmokeRootStartup.Completed ? "Blackhole Framework: root module startup succeeded." : "Blackhole Framework: starting root module...");
        }
    }

    public sealed class SmokeRootStartup : RootModuleStartup
    {
        public static bool Completed { get; private set; }

        public SmokeRootStartup(RootModuleStartupRegistry registry) : base(registry) => Completed = false;

        public override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Completed = true;
            Debug.Log("Blackhole development smoke module started successfully.");
            return UniTask.CompletedTask;
        }
    }
}
