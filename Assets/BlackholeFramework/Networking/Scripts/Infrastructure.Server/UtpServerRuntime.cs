using Blackhole.Messaging.Contract;
using Blackhole.Networking.Contract;
using Blackhole.SharedKernel.Application;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace Blackhole.Networking.Infrastructure.Server
{
    public sealed class UtpServerRuntime :INetworkServer, IApplicationInit, IApplicationTick, IDisposable
    {
        private const int MaxPayloadBytes = 128;

        private readonly NetworkSessionOptions _options;
        private readonly INetworkPayloadHandler _payloadHandler;
        private readonly IEventPublisher<NetworkPeerConnected> _connected;
        private readonly IEventPublisher<NetworkPeerDisconnected> _disconnected;

        private readonly List<PeerConnection> _peers = new();

        private NetworkDriver _driver;
        private NetworkPipeline _reliablePipeline;

        private int _nextPeerId = 1;
        private bool _running;
        private bool _disposed;

        public UtpServerRuntime(
            NetworkSessionOptions options,
            INetworkPayloadHandler payloadHandler,
            IEventPublisher<NetworkPeerConnected> connected,
            IEventPublisher<NetworkPeerDisconnected> disconnected)
        {
            _options = options;
            _payloadHandler = payloadHandler;
            _connected = connected;
            _disconnected = disconnected;
        }

        public void Init()
        {
            if (_disposed || _driver.IsCreated)
                return;

            if (!NetworkEndpoint.TryParse(_options.Address.Host, _options.Address.Port, out var endpoint))
                throw new InvalidOperationException("The configured server bind address is invalid.");

            _driver = NetworkDriver.Create();
            _reliablePipeline = _driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

            if (_driver.Bind(endpoint) != 0)
            {
                Debug.LogError("UTP server bind failed.");
                Dispose();
                return;
            }

            if (_driver.Listen() != 0)
            {
                Debug.LogError("UTP server listen failed.");
                Dispose();
                return;
            }

            _running = true;
            Debug.Log($"Server listening: {endpoint}");
        }

        public void Tick(float deltaTime)
        {
            if (!_running)
                return;

            _driver.ScheduleUpdate().Complete();

            RemoveStaleConnections();
            AcceptConnections();
            ProcessEvents();
        }

        private void AcceptConnections()
        {
            NetworkConnection connection;

            while ((connection = _driver.Accept()) != default)
            {
                var peerId = new NetworkPeerId(_nextPeerId++);

                _peers.Add(new PeerConnection(peerId, connection));

                _connected.Publish(new NetworkPeerConnected(peerId));

                Debug.Log($"Peer {peerId.Id} connected.");
            }
        }

        private void ProcessEvents()
        {
            for (var i = 0; i < _peers.Count; i++)
            {
                var peer = _peers[i];
                NetworkEvent.Type networkEvent;

                while ((networkEvent = peer.Connection.PopEvent(_driver, out var reader))
                    != NetworkEvent.Type.Empty)
                {
                    if (networkEvent == NetworkEvent.Type.Data)
                    {
                        ProcessData(peer.PeerId, reader);
                    }
                    else if (networkEvent == NetworkEvent.Type.Disconnect)
                    {
                        _disconnected.Publish(
                            new NetworkPeerDisconnected(peer.PeerId));

                        peer.Connection = default;
                        break;
                    }
                }
            }
        }

        private void ProcessData(NetworkPeerId peerId, DataStreamReader reader)
        {
            if (reader.Length == 0 || reader.Length > MaxPayloadBytes)
                return;

            var bytes = new byte[reader.Length];
            reader.ReadBytes(bytes.AsSpan());

            if (reader.HasFailedReads)
                return;

            _payloadHandler.Handle(peerId, new NetworkPayload(bytes));
        }

        public bool TrySend(NetworkPeerId peerId, NetworkPayload payload)
        {
            if (!_running)
                return false;

            foreach (var peer in _peers)
            {
                if (peer.PeerId.Equals(peerId) && peer.Connection.IsCreated)
                    return SendInternal(peer.Connection, payload);
            }

            return false;
        }

        public bool TryBroadcast(NetworkPayload payload)
        {
            if (!_running)
                return false;

            var success = true;

            foreach (var peer in _peers)
            {
                if (!peer.Connection.IsCreated)
                    continue;

                if (!SendInternal(peer.Connection, payload))
                    success = false;
            }

            return success;
        }

        private bool SendInternal(NetworkConnection connection, NetworkPayload payload)
        {
            if (payload.Data.Length == 0 || payload.Data.Length > MaxPayloadBytes)
                return false;

            var result = _driver.BeginSend(_reliablePipeline, connection, out var writer, payload.Data.Length);

            if (result != 0)
                return false;

            var data = payload.Data.Span;

            for (var i = 0; i < data.Length; i++)
                writer.WriteByte(data[i]);

            if (writer.HasFailedWrites)
            {
                _driver.AbortSend(writer);
                return false;
            }

            return _driver.EndSend(writer) >= 0;
        }

        private void RemoveStaleConnections()
        {
            for (var i = _peers.Count - 1; i >= 0; i--)
            {
                if (!_peers[i].Connection.IsCreated)
                    _peers.RemoveAt(i);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _running = false;

            if (!_driver.IsCreated)
                return;

            foreach (var peer in _peers)
            {
                if (peer.Connection.IsCreated)
                    peer.Connection.Disconnect(_driver);
            }

            _driver.ScheduleUpdate().Complete();
            _driver.Dispose();
            _driver = default;

            _peers.Clear();
            _running = false;
        }

        private sealed class PeerConnection
        {
            public NetworkPeerId PeerId { get; }
            public NetworkConnection Connection;

            public PeerConnection(NetworkPeerId peerId, NetworkConnection connection)
            {
                PeerId = peerId;
                Connection = connection;
            }
        }
    }
}
