using Blackhole.SceneLoader.Contract;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using Blackhole.ModuleSystem.Contract;
using Blackhole.SharedKernel.Composition;

namespace Blackhole.ModuleSystem.Composition
{
    public class RuntimeModuleLoader : IModuleLoader, IDisposable
    {
        private readonly LifetimeScope projectScope;
        private readonly ModuleManifestCatalogSO moduleCatalog;
        private readonly ModuleResolver moduleResolver;
        private readonly ISceneTransitionUseCase sceneLoader;

        private ActiveModuleSession activeSession;

        public RuntimeModuleLoader(LifetimeScope projectScope, ModuleManifestCatalogSO moduleCatalog, ModuleResolver moduleResolver, ISceneTransitionUseCase sceneLoader)
        {
            this.projectScope = projectScope;
            this.moduleCatalog = moduleCatalog;
            this.moduleResolver = moduleResolver;
            this.sceneLoader = sceneLoader;
        }

        public UniTask LoadRootModuleAsync(string rootModuleID, CancellationToken cancellationToken)
        {
            return LoadRootModuleAsync(rootModuleID, NoModuleEntryOptions.Instance, cancellationToken);
        }

        public async UniTask LoadRootModuleAsync<TEntryOptions>(string rootModuleID, TEntryOptions entryOptions, CancellationToken cancellationToken) where TEntryOptions : class
        {
            if (string.IsNullOrWhiteSpace(rootModuleID))
                throw new ArgumentException("Root module ID cannot be null or whitespace.", nameof(rootModuleID));

            if (entryOptions == null)
                throw new ArgumentNullException(nameof(entryOptions));

            await UnloadCurrentRootModuleAsync(cancellationToken);

            InstalledModules parentModules = projectScope.Container.Resolve<InstalledModules>();
            ResolverDto resolverDto = moduleResolver.GetResolverDto(rootModuleID, parentModules);

            LifetimeScope sessionScope = CreateSessionScope(rootModuleID, resolverDto, entryOptions);
            ScopeCancellation scopeCancellation = sessionScope.Container.Resolve<ScopeCancellation>();
            RootModuleStartupRegistry startupRegistry = sessionScope.Container.Resolve<RootModuleStartupRegistry>();

            List<string> loadedScenes = new();
            ActiveModuleSession session = null;

            try
            {
                foreach (string scene in resolverDto.Scenes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (LifetimeScope.EnqueueParent(sessionScope))
                    {
                        await sceneLoader.LoadSceneAsync(scene, true, cancellationToken);
                    }
                    loadedScenes.Add(scene);
                }

                string rootScenePath = moduleCatalog.GetModule(rootModuleID).ModuleScene;
                if (!string.IsNullOrWhiteSpace(rootScenePath))
                {
                    Scene rootScene = SceneManager.GetSceneByPath(rootScenePath);
                    if (!rootScene.IsValid())
                        rootScene = SceneManager.GetSceneByName(rootScenePath);

                    if (!rootScene.IsValid() || !rootScene.isLoaded || !SceneManager.SetActiveScene(rootScene))
                        throw new InvalidOperationException($"Root module '{rootModuleID}' could not activate scene '{rootScenePath}'.");
                }

                // Startup may immediately navigate to another root module, so this session must be unloadable first.
                session = new ActiveModuleSession(rootModuleID, sessionScope, scopeCancellation, loadedScenes);
                activeSession = session;

                IRootModuleStartup rootModuleStartup = startupRegistry.GetRequired(rootModuleID);
                using (CancellationTokenSource startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, scopeCancellation.Token))
                {
                    startupCancellation.Token.ThrowIfCancellationRequested();
                    await rootModuleStartup.InitializeAsync(startupCancellation.Token);
                }
            }
            catch
            {
                if (ReferenceEquals(activeSession, session))
                    activeSession = null;

                scopeCancellation.Cancel();

                try
                {
                    await RollbackLoadedScenesAsync(loadedScenes);
                }
                finally
                {
                    sessionScope.Dispose();
                }

                throw;
            }
        }

        public async UniTask UnloadCurrentRootModuleAsync(CancellationToken ct)
        {
            ActiveModuleSession session = activeSession;

            if (session == null)
                return;

            try
            {
                for (int i = session.LoadedScenes.Count - 1; i >= 0; i--)
                {
                    string sceneName = session.LoadedScenes[i];
                    await sceneLoader.UnloadSceneAsync(sceneName, ct);
                }
            }
            finally
            {
                session.ScopeCancellation.Cancel();
                session.SessionScope.Dispose();
                activeSession = null;
            }
        }

        private LifetimeScope CreateSessionScope<TEntryOptions>(string rootModuleID, ResolverDto resolverDto, TEntryOptions entryOptions) where TEntryOptions : class
        {
            bool isPersistentRoot = moduleCatalog.GetModule(rootModuleID).IsPersistant;
            string scopeName = isPersistentRoot ? $"Persistent Root Module Scope ({rootModuleID})" : $"Root Module Scope ({rootModuleID})";
            return projectScope.CreateChild(builder =>
            {
                ScopeCancellationInstaller.Install(builder);
                builder.Register<RootModuleStartupRegistry>(Lifetime.Singleton);

                if (isPersistentRoot)
                    ModuleSystemInstaller.RegisterServices(builder);

                builder.RegisterInstance(new ModuleSessionInfo(rootModuleID));
                builder.RegisterInstance(resolverDto);
                builder.RegisterInstance(new InstalledModules(resolverDto.ModuleIDs, projectScope.Container.Resolve<InstalledModules>()));
                builder.RegisterInstance<TEntryOptions>(entryOptions);

                foreach (var installer in resolverDto.Installers)
                {
                    installer.InstallModule(builder);
                }
            }, scopeName);
        }

        private async UniTask RollbackLoadedScenesAsync(List<string> loadedScenes)
        {
            for (int i = loadedScenes.Count - 1; i >= 0; i--)
            {
                try
                {
                    string sceneName = loadedScenes[i];
                    // Cleanup must complete even when the original load operation was cancelled.
                    await sceneLoader.UnloadSceneAsync(sceneName, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to unload scene '{loadedScenes[i]}' during rollback: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            activeSession?.ScopeCancellation.Cancel();
            activeSession?.SessionScope.Dispose();
            activeSession = null;
        }

        private sealed class NoModuleEntryOptions
        {
            public static NoModuleEntryOptions Instance { get; } = new();

            private NoModuleEntryOptions()
            {
            }
        }
    }

    public sealed class ModuleSessionInfo
    {
        public string RootModuleID { get; }
        public ModuleSessionInfo(string rootModuleID)
        {
            RootModuleID = rootModuleID;
        }
    }

    internal sealed class ActiveModuleSession
    {
        public string RootModuleID { get; }
        public LifetimeScope SessionScope { get; }
        public ScopeCancellation ScopeCancellation { get; }
        public IReadOnlyList<string> LoadedScenes { get; }

        public ActiveModuleSession(
            string rootModuleID,
            LifetimeScope sessionScope,
            ScopeCancellation scopeCancellation,
            IReadOnlyList<string> loadedScenes)
        {
            RootModuleID = rootModuleID;
            SessionScope = sessionScope;
            ScopeCancellation = scopeCancellation;
            LoadedScenes = loadedScenes;
        }
    }
}
