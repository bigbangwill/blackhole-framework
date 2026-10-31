#if UNITY_EDITOR
using System.Collections;
using Blackhole.Networking.Contract;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Unity;

namespace Blackhole.Development.Networking.Tests
{
    public sealed class NetworkingLifecycleTests
    {
        [UnityTest]
        public IEnumerator ScopeLifecycleInitializesAndTicksTheTransport()
        {
            ushort port = NetworkingTestSession.FreePort();
            var received = new NetworkRecorder();
            LifetimeScope server = LifetimeScope.Create(builder => NetworkingTestSession.Install(builder, NetworkRole.Server, port, received));
            LifetimeScope client = LifetimeScope.Create(builder => NetworkingTestSession.Install(builder, NetworkRole.Client, port, new NetworkRecorder()));
            try
            {
                INetworkClient transport = client.Container.Resolve<INetworkClient>();
                Assert.That(transport.Connect(new NetworkAddress("127.0.0.1", port)).IsSuccess, Is.True);
                float deadline = Time.realtimeSinceStartup + 10;
                while (transport.State != NetworkConnectionState.Connected && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(transport.State, Is.EqualTo(NetworkConnectionState.Connected));
                Assert.That(transport.Send(new NetworkPayload(new byte[] { 21, 42 })).IsSuccess, Is.True);
                while (received.Payloads.Count == 0 && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(received.Payloads.Count, Is.EqualTo(1));
                Assert.That(received.Payloads[0], Is.EqualTo(new byte[] { 21, 42 }));
            }
            finally
            {
                client.Dispose();
                server.Dispose();
                Object.Destroy(client.gameObject);
                Object.Destroy(server.gameObject);
            }
        }
    }
}
#endif
