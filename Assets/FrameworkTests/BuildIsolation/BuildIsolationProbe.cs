#if BLACKHOLE_BUILD_AUDIT && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Blackhole.Framework.Composition;
using Blackhole.Messaging.Contract;
using Blackhole.ModuleSystem.Composition;
using Blackhole.Networking.Contract;
using Blackhole.SharedKernel.Application;
using Blackhole.SharedKernel.Composition;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Blackhole.Development.BuildIsolation
{
    public sealed class BuildIsolationProbe : MonoBehaviour
    {
#if UNITY_SERVER
        private const string Role = "Server";
        private const string Opposite = "Client";
#else
        private const string Role = "Client";
        private const string Opposite = "Server";
#endif
        private static readonly byte[] Payload = { 66, 72, 70, 87, 1, 3, 7 };
        private readonly AuditResult result = new();
        private IObjectResolver container;
        private ModuleInstallerSO installer;
        private Recorder recorder;
        private INetworkClient client;
        private bool sent;
        private float receivedAt = -1;
        private readonly List<IApplicationTick> ticks = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Begin()
        {
            var runner = new GameObject("Build Isolation Verification");
            DontDestroyOnLoad(runner);
            runner.AddComponent<BuildIsolationProbe>();
        }

        private IEnumerator Start()
        {
            Application.runInBackground = true;
            Application.logMessageReceived += OnLog;
            result.role = Role;
            FrameworkLifetimeScope framework = null;
            float deadline = Time.realtimeSinceStartup + 25;
            while (Time.realtimeSinceStartup < deadline)
            {
                framework = FindFirstObjectByType<FrameworkLifetimeScope>();
                if (framework != null && framework.Container != null)
                {
                    var settings = framework.Container.Resolve<BlackholeProjectSettingsSO>();
                    if (settings.StartingModule != null && SceneManager.GetActiveScene().path == settings.StartingModule.ModuleScene)
                    {
                        result.frameworkStarted = true;
                        break;
                    }
                }
                yield return null;
            }
            if (!result.frameworkStarted) result.errors.Add("Framework did not activate its configured root scene.");
            try { InitializeTransport(framework); }
            catch (Exception exception) { result.errors.Add(exception.ToString()); Finish(); yield break; }

            Debug.Log("BLACKHOLE_AUDIT_READY " + Role);
            deadline = Time.realtimeSinceStartup + 75;
            while (Time.realtimeSinceStartup < deadline)
            {
                try
                {
                    foreach (var tick in ticks) tick.Tick(Time.unscaledDeltaTime);
                    if (client != null && client.State == NetworkConnectionState.Connected && !sent)
                    {
                        sent = client.Send(new NetworkPayload(Payload)).IsSuccess;
                        result.payloadSent = sent;
                    }
                    if (recorder.received && receivedAt < 0) receivedAt = Time.realtimeSinceStartup;
                }
                catch (Exception exception) { result.errors.Add(exception.ToString()); break; }
                if (receivedAt >= 0 && Time.realtimeSinceStartup - receivedAt > 1) break;
                yield return null;
            }
            result.payloadReceived = recorder.received;
            if (!recorder.received) result.errors.Add("Client/server payload exchange timed out.");
            Finish();
        }

        private void InitializeTransport(FrameworkLifetimeScope framework)
        {
            var settings = framework.Container.Resolve<BlackholeProjectSettingsSO>();
            var manifest = settings.ModuleCatalog.GetModule("Blackhole.Networking");
            installer = Instantiate(manifest.Installer);
            result.selectedInstaller = installer.GetType().FullName;
            if (installer.GetType().Assembly.GetName().Name != "Blackhole.Networking.Composition." + Role)
                result.errors.Add("Manifest selected the wrong role's installer.");

            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            result.oppositeInstallerFieldPresent = manifest.GetType().GetField("_" + Opposite.ToLowerInvariant() + "Installer", flags) != null;
            if (result.oppositeInstallerFieldPresent) result.errors.Add("Manifest retains the opposite role's serialized field.");

            string managed = Path.Combine(Application.dataPath, "Managed");
            result.assemblies = Directory.GetFiles(managed, "*.dll").Select(Path.GetFileNameWithoutExtension).OrderBy(name => name).ToArray();
            foreach (string name in new[] { "Blackhole.Networking.Composition." + Opposite, "Blackhole.Networking.Infrastructure." + Opposite })
            {
                if (result.assemblies.Contains(name)) result.errors.Add("Forbidden assembly exists: " + name);
                try { Assembly.Load(name); result.errors.Add("Forbidden assembly can be loaded: " + name); }
                catch (FileNotFoundException) { }
            }
            installer.GetType().GetField("host", flags).SetValue(installer, "127.0.0.1");
            ushort port = ushort.Parse(Argument("-blackhole-audit-port", "47788"));
            installer.GetType().GetField("port", flags).SetValue(installer, port);
            recorder = new Recorder();
            var builder = new ContainerBuilder();
            builder.RegisterInstance<INetworkPayloadHandler>(recorder);
            builder.RegisterInstance<INetworkClientPayloadHandler>(recorder);
            builder.RegisterInstance<IEventPublisher<NetworkPeerConnected>>(new Publisher<NetworkPeerConnected>());
            builder.RegisterInstance<IEventPublisher<NetworkPeerDisconnected>>(new Publisher<NetworkPeerDisconnected>());
            builder.RegisterInstance<IEventPublisher<NetworkConnectionStateChanged>>(new Publisher<NetworkConnectionStateChanged>());
            installer.InstallModule(builder);
            container = builder.Build();
            var options = container.Resolve<NetworkSessionOptions>();
            result.optionsRole = options.Role.ToString();
            var init = container.Resolve<IApplicationInit>();
            ticks.Add(container.Resolve<IApplicationTick>());
            result.runtimeType = init.GetType().FullName;
            if (init.GetType().Assembly.GetName().Name != "Blackhole.Networking.Infrastructure." + Role)
                result.errors.Add("Wrong transport implementation registered.");
            init.Init();
#if UNITY_SERVER
            recorder.server = container.Resolve<INetworkServer>();
            if (container.TryResolve<INetworkClient>(out _)) result.errors.Add("Server container exposes a client implementation.");
#else
            client = container.Resolve<INetworkClient>();
            if (container.TryResolve<INetworkServer>(out _)) result.errors.Add("Client container exposes a server implementation.");
            if (client.Connect(new NetworkAddress("127.0.0.1", port)).IsFailure) result.errors.Add("Client Connect failed.");
#endif
        }

        private void Finish()
        {
            container?.Dispose();
            if (installer != null) Destroy(installer);
            Application.logMessageReceived -= OnLog;
            result.passed = result.errors.Count == 0 && result.frameworkStarted && result.payloadReceived;
            string path = Argument("-blackhole-audit-result", Path.Combine(Application.persistentDataPath, "build-isolation-" + Role + ".json"));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(result, true));
            Debug.Log("BLACKHOLE_AUDIT_FINISHED " + Role + " " + result.passed);
            Application.Quit(result.passed ? 0 : 17);
        }

        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) result.errors.Add(message);
        }

        private static string Argument(string key, string fallback)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, key);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
        }

        private sealed class Recorder : INetworkPayloadHandler, INetworkClientPayloadHandler
        {
            public bool received;
            public INetworkServer server;
            public void Handle(NetworkPayload payload) => received = payload.Data.ToArray().SequenceEqual(Payload);
            public void Handle(NetworkPeerId peer, NetworkPayload payload)
            {
                Handle(payload);
                if (received) server.TrySend(peer, payload);
            }
        }

        private sealed class Publisher<T> : IEventPublisher<T>
        {
            public void Publish(T message) { }
        }

        [Serializable]
        private sealed class AuditResult
        {
            public bool passed;
            public string role;
            public string selectedInstaller;
            public string runtimeType;
            public string optionsRole;
            public bool frameworkStarted;
            public bool oppositeInstallerFieldPresent;
            public bool payloadSent;
            public bool payloadReceived;
            public string[] assemblies;
            public List<string> errors = new();
        }
    }
}
#endif
