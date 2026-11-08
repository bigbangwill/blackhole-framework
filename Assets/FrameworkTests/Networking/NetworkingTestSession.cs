#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Blackhole.Messaging.Contract;
using Blackhole.Networking.Composition.Client;
using Blackhole.Networking.Composition.Server;
using Blackhole.Networking.Contract;
using Blackhole.RuntimeLifecycle.Infrastructure;
using Blackhole.SharedKernel.Composition;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace Blackhole.Development.Networking.Tests
{
    public sealed class NetworkRecorder : INetworkPayloadHandler, INetworkClientPayloadHandler
    {
        public readonly List<NetworkPeerId> Peers = new();
        public readonly List<NetworkConnectionStateChanged> States = new();
        public readonly List<byte[]> Payloads = new();
        public NetworkPeerId LastSender;
        public void Handle(NetworkPayload payload) => Payloads.Add(payload.Data.ToArray());
        public void Handle(NetworkPeerId peer, NetworkPayload payload) { LastSender = peer; Handle(payload); }
    }

    public static class NetworkingTestSession
    {
        public static ushort FreePort()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            return (ushort)((IPEndPoint)socket.LocalEndPoint).Port;
        }

        public static void Install(IContainerBuilder builder, NetworkRole role, ushort port, NetworkRecorder recorder)
        {
            builder.RegisterInstance<INetworkPayloadHandler>(recorder);
            builder.RegisterInstance<INetworkClientPayloadHandler>(recorder);
            builder.RegisterInstance<IEventPublisher<NetworkPeerConnected>>(new Publisher<NetworkPeerConnected>(message => recorder.Peers.Add(message.PeerId)));
            builder.RegisterInstance<IEventPublisher<NetworkPeerDisconnected>>(new Publisher<NetworkPeerDisconnected>(_ => { }));
            builder.RegisterInstance<IEventPublisher<NetworkConnectionStateChanged>>(new Publisher<NetworkConnectionStateChanged>(message => recorder.States.Add(message)));
            InstallTransport(builder, role, port);
            AssetDatabase.LoadAssetAtPath<RuntimeLifecyleInstaller>("Assets/BlackholeFramework/RuntimeLifecycle/RuntimeLifecyleInstaller.asset").InstallModule(builder);
        }

        public static void InstallTransport(IContainerBuilder builder, NetworkRole role, ushort port)
        {
            string name = role == NetworkRole.Server ? "NetworkingServerInstaller" : "NetworkingInstaller";
            ModuleInstallerSO original = role == NetworkRole.Server
                ? AssetDatabase.LoadAssetAtPath<NetworkingServerInstaller>($"Assets/BlackholeFramework/Networking/Data/{name}.asset")
                : AssetDatabase.LoadAssetAtPath<NetworkingClientInstaller>($"Assets/BlackholeFramework/Networking/Data/{name}.asset");
            var copy = UnityEngine.Object.Instantiate(original);
            try
            {
                SerializedObject serialized = new(copy);
                serialized.FindProperty("host").stringValue = "127.0.0.1";
                serialized.FindProperty("port").intValue = port;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                copy.InstallModule(builder);
            }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
        }

        private sealed class Publisher<T> : IEventPublisher<T>
        {
            private readonly Action<T> receive;
            public Publisher(Action<T> receive) => this.receive = receive;
            public void Publish(T message) => receive(message);
        }
    }
}
#endif
