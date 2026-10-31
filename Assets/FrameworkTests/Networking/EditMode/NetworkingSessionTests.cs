using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Blackhole.Networking.Contract;
using Blackhole.SharedKernel.Application;
using NUnit.Framework;
using VContainer;
using VContainer.Unity;

namespace Blackhole.Development.Networking.Tests
{
    public sealed class NetworkingSessionTests
    {
        [TestCase(-1, "127.0.0.1", 7788)]
        [TestCase(0, "", 7788)]
        [TestCase(0, null, 7788)]
        [TestCase(0, "127.0.0.1", 0)]
        public void InvalidSessionOptionsAreRejected(int role, string host, int port)
        {
            Assert.That(() => new NetworkSessionOptions((NetworkRole)role, host, (ushort)port), Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void RoleContainersAreIsolatedAndChildrenShareTheirOwnTransport()
        {
            ushort port = NetworkingTestSession.FreePort();
            using var server = Build(NetworkRole.Server, port, new NetworkRecorder());
            using var client = Build(NetworkRole.Client, port, new NetworkRecorder());
            using var otherClient = Build(NetworkRole.Client, port, new NetworkRecorder());
            using var child = client.CreateScope();
            Assert.That(server.TryResolve<INetworkClient>(out _), Is.False);
            Assert.That(client.TryResolve<INetworkServer>(out _), Is.False);
            Assert.That(child.Resolve<INetworkClient>(), Is.SameAs(client.Resolve<INetworkClient>()));
            Assert.That(otherClient.Resolve<INetworkClient>(), Is.Not.SameAs(client.Resolve<INetworkClient>()));
            var builder = new ContainerBuilder();
            NetworkingTestSession.InstallTransport(builder, NetworkRole.Client, port);
            Assert.Throws<InvalidOperationException>(() => NetworkingTestSession.InstallTransport(builder, NetworkRole.Server, port));
        }

        [Test]
        public void ClientsAndServerExchangeReliablePayloadsOverRealSockets()
        {
            ushort port = NetworkingTestSession.FreePort();
            var serverMessages = new NetworkRecorder();
            var firstMessages = new NetworkRecorder();
            var secondMessages = new NetworkRecorder();
            using var server = Build(NetworkRole.Server, port, serverMessages);
            using var first = Build(NetworkRole.Client, port, firstMessages);
            using var second = Build(NetworkRole.Client, port, secondMessages);
            INetworkClient client = first.Resolve<INetworkClient>();
            Assert.That(client.Send(new NetworkPayload(new byte[] { 1 })).IsFailure, Is.True);
            Assert.That(client.Connect(new NetworkAddress("127.0.0.1", port)).IsSuccess, Is.True);
            Assert.That(second.Resolve<INetworkClient>().Connect(new NetworkAddress("127.0.0.1", port)).IsSuccess, Is.True);
            PumpUntil(() => serverMessages.Peers.Count == 2 && client.State == NetworkConnectionState.Connected && second.Resolve<INetworkClient>().State == NetworkConnectionState.Connected, server, first, second);
            Assert.That(firstMessages.States[0].Current, Is.EqualTo(NetworkConnectionState.Connecting));
            byte[] payload = { 5, 10, 15, 20 };
            Assert.That(client.Send(new NetworkPayload(payload)).IsSuccess, Is.True);
            PumpUntil(() => serverMessages.Payloads.Count == 1, server, first, second);
            Assert.That(serverMessages.Payloads[0], Is.EqualTo(payload));
            INetworkServer transport = server.Resolve<INetworkServer>();
            Assert.That(transport.TrySend(serverMessages.LastSender, new NetworkPayload(payload)), Is.True);
            PumpUntil(() => firstMessages.Payloads.Count == 1, server, first, second);
            Assert.That(firstMessages.Payloads[0], Is.EqualTo(payload));
            Assert.That(secondMessages.Payloads, Is.Empty);
            Assert.That(transport.TryBroadcast(new NetworkPayload(new byte[] { 99 })), Is.True);
            PumpUntil(() => firstMessages.Payloads.Count == 2 && secondMessages.Payloads.Count == 1, server, first, second);
            Assert.That(transport.TrySend(serverMessages.LastSender, new NetworkPayload(Array.Empty<byte>())), Is.False);
            Assert.That(transport.TrySend(serverMessages.LastSender, new NetworkPayload(new byte[129])), Is.False);
            Assert.That(client.Disconnect().IsSuccess, Is.True);
            Assert.That(client.State, Is.EqualTo(NetworkConnectionState.Disconnected));
        }

        [Test]
        public void DisposingServerReleasesThePortAndTransportCallbacksRemainSafe()
        {
            ushort port = NetworkingTestSession.FreePort();
            var previous = Build(NetworkRole.Server, port, new NetworkRecorder());
            var initialize = previous.Resolve<IReadOnlyList<IApplicationInit>>();
            var ticks = previous.Resolve<IReadOnlyList<IApplicationTick>>();
            previous.Dispose();
            foreach (var init in initialize) Assert.DoesNotThrow(() => init.Init());
            foreach (var tick in ticks) Assert.DoesNotThrow(() => tick.Tick(0.01f));
            var messages = new NetworkRecorder();
            using var server = Build(NetworkRole.Server, port, messages);
            using var client = Build(NetworkRole.Client, port, new NetworkRecorder());
            Assert.That(client.Resolve<INetworkClient>().Connect(new NetworkAddress("127.0.0.1", port)).IsSuccess, Is.True);
            PumpUntil(() => messages.Peers.Count == 1, server, client);
        }

        private static IObjectResolver Build(NetworkRole role, ushort port, NetworkRecorder recorder)
        {
            var builder = new ContainerBuilder();
            NetworkingTestSession.Install(builder, role, port, recorder);
            return builder.Build();
        }

        private static void PumpUntil(Func<bool> condition, params IObjectResolver[] sessions)
        {
            var timer = Stopwatch.StartNew();
            while (!condition() && timer.ElapsedMilliseconds < 5000)
            {
                foreach (var session in sessions)
                    foreach (var tick in session.Resolve<IReadOnlyList<ITickable>>()) tick.Tick();
                Thread.Sleep(1);
            }
            Assert.That(condition(), Is.True, "Networking did not deliver the expected state within five seconds.");
        }
    }
}
