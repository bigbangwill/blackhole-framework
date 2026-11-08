using Blackhole.Messaging.Contract;
using Blackhole.Networking.Contract;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blackhole.Multiplayer.Application.Server
{
    public class ConnectedPlayerRegistry : IDisposable
    {
        private readonly List<ConnectedPlayer> _connectedPlayers = new();

        private readonly IDisposable _connectedSub;
        private readonly IDisposable _disconnectedSub;

        public ConnectedPlayerRegistry(
            IEventSubscriber<NetworkPeerConnected> connectedSub,
            IEventSubscriber<NetworkPeerDisconnected> disconnectedSub)
        {
            _connectedSub = connectedSub.Subscribe(OnPlayerConnected);
            _disconnectedSub = disconnectedSub.Subscribe(OnPlayerDisconnected);
            Debug.Log("Staretdf");
        }
        
        private void OnPlayerConnected(NetworkPeerConnected connected)
        {
            ConnectedPlayer connectedPlayer = new(connected.PeerId);
            if (_connectedPlayers.Contains(connectedPlayer))
            {
                Debug.LogError($"Player is already connected: {connected}");
                return;
            }

            _connectedPlayers.Add(connectedPlayer);
            Debug.Log("Client Connected");
        }

        private void OnPlayerDisconnected(NetworkPeerDisconnected disconnected)
        {
            ConnectedPlayer disconnectedPlayer = new(disconnected.PeerId);
            if (!_connectedPlayers.Contains(disconnectedPlayer))
            {
                Debug.LogError($"Player doesnt exists in list {disconnected.PeerId}");
                return;
            }
            _connectedPlayers.Remove(disconnectedPlayer);
            Debug.Log("Client Disconnected");
        }

        public void Dispose()
        {
            _connectedSub.Dispose();
            _disconnectedSub.Dispose();
        }
    }

    public class ConnectedPlayer
    {
        public NetworkPeerId PeerId { get; }

        public ConnectedPlayer(NetworkPeerId peerId)
        {
            PeerId = peerId;
        }

        public bool Equals(ConnectedPlayer other) => PeerId == other.PeerId;

        public override bool Equals(object obj) => obj is ConnectedPlayer other && Equals(other);

        public static bool operator ==(ConnectedPlayer left, ConnectedPlayer right) => left.Equals(right);
        public static bool operator !=(ConnectedPlayer left, ConnectedPlayer right) => !left.Equals(right);

        public override int GetHashCode() => PeerId.GetHashCode();
        public override string ToString() => PeerId.ToString();
    }
}