# Networking

Blackhole.Networking provides transport contracts and Unity Transport implementations. It includes no game protocol, match rules, player model or lesson scenes. Install Unity Transport 6.5.0 alongside the dependencies in the framework Version.json.

## Session ownership

Install exactly one role per owning container. Data/NetworkingInstaller.asset configures a client; Data/NetworkingServerInstaller.asset configures a server. Each installer registers immutable NetworkSessionOptions and the corresponding transport as a singleton shared by child scopes. Its driver is disposed with the owning scope.

The module manifest depends on Blackhole.Messaging and uses the client installer by default. For a server session, explicitly install the server asset in the server scope rather than adding the default client installer to that same scope. The installers reject a second role in one container.

Register INetworkClientPayloadHandler for a client, or INetworkPayloadHandler for a server. Register these alongside the transport in its owning scope. Application-specific handlers are supplied by the receiving game.

Install the existing RuntimeLifecyleInstaller in the same scope as the transport. Its ContainerLocal collections initialize and tick local IApplicationInit/IApplicationTick implementations. An ancestor's lifecycle runner does not tick services registered in a child scope. Messaging must provide the connection/peer IEventPublisher contracts through that scope or its ancestors.

## Connection and data

After initialization, resolve INetworkClient and call Connect with the session address. Send and Disconnect return Result values. Client connection state changes publish NetworkConnectionStateChanged.

The server binds and listens during initialization. INetworkServer supports TrySend for one NetworkPeerId and TryBroadcast for connected peers. NetworkPeerConnected and NetworkPeerDisconnected identify peer lifecycle changes.

Payloads use the reliable sequenced pipeline. The server currently accepts and sends non-empty payloads of at most 128 bytes. Sending larger messages requires a deliberate protocol/transport change; no fragmentation or application serialization is supplied here. Raw payload handlers define the application protocol.

The supplied defaults use local development addresses on port 7788. Configure the installer assets for the receiving project and preserve those values during upgrades. Networking is only instantiated when its installer is used; importing the module does not start a server or connect a client.
