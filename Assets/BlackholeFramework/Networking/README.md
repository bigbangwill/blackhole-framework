# Networking

Blackhole.Networking provides transport contracts and Unity Transport implementations. It includes no game protocol, match rules, player model or lesson scenes. Install Unity Transport 6.5.0 alongside the dependencies in the framework Version.json.

## Session ownership

Install exactly one role per owning container. Data/NetworkingInstaller.asset configures a client; Data/NetworkingServerInstaller.asset configures a server. Each installer registers immutable NetworkSessionOptions and the corresponding transport as a singleton shared by child scopes. Its driver is disposed with the owning scope.

The platform module manifest depends on Blackhole.Messaging and selects the client installer for ordinary players or the server installer when UNITY_SERVER is defined. In the Editor, it selects the server installer when the instance's Multiplayer Role is Server; Client or Client and Server selects the client installer. Both installers remain available for local client/server tests. The installers reject a second role in one container.

For testing without player builds, open Window > Play Mode > Scenarios, keep the Main Editor as Client and add an Additional Editor Instance with its Multiplayer Role set to Server. Leave Local Instances empty. The Dedicated Server package supplies the role settings; Multiplayer Play Mode runs the additional instance as a virtual Editor player.

## Assembly separation

Scripts/Composition.Client and Scripts/Composition.Server contain NetworkingClientInstaller and NetworkingServerInstaller. Each installer inherits directly from ModuleInstallerSO, owns its endpoint configuration and references only its matching infrastructure assembly.

Scripts/Infrastructure.Client contains UtpClientRuntime; Scripts/Infrastructure.Server contains UtpServerRuntime. Their assembly names and namespaces are Blackhole.Networking.Infrastructure.Client and Blackhole.Networking.Infrastructure.Server. Contract and Application remain shared. There is no shared networking composition or transport implementation assembly.

Client composition and infrastructure assemblies require UNITY_EDITOR || !UNITY_SERVER. Server composition and infrastructure assemblies require UNITY_EDITOR || UNITY_SERVER. Both roles compile in the Editor, while player compilation includes only the matching role. Use the Dedicated Server build subtarget to define UNITY_SERVER; selecting a desktop operating system alone does not select the networking role. Networking tests run in the Editor because they exercise both roles in one process. PlatformModuleManifestSO uses matching compile guards on its serialized fields: both references remain editable in the Editor, but each player contains only its selected installer field and asset dependency.

Register INetworkClientPayloadHandler for a client, or INetworkPayloadHandler for a server. Register these alongside the transport in its owning scope. Application-specific handlers are supplied by the receiving game.

Install the existing RuntimeLifecyleInstaller in the same scope as the transport. Its ContainerLocal collections initialize and tick local IApplicationInit/IApplicationTick implementations. An ancestor's lifecycle runner does not tick services registered in a child scope. Messaging must provide the connection/peer IEventPublisher contracts through that scope or its ancestors.

## Connection and data

After initialization, resolve INetworkClient and call Connect with the session address. Send and Disconnect return Result values. Client connection state changes publish NetworkConnectionStateChanged.

The server binds and listens during initialization. INetworkServer supports TrySend for one NetworkPeerId and TryBroadcast for connected peers. NetworkPeerConnected and NetworkPeerDisconnected identify peer lifecycle changes.

Payloads use the reliable sequenced pipeline. The server currently accepts and sends non-empty payloads of at most 128 bytes. Sending larger messages requires a deliberate protocol/transport change; no fragmentation or application serialization is supplied here. Raw payload handlers define the application protocol.

The supplied defaults use local development addresses on port 7788. Configure the installer assets for the receiving project and preserve those values during upgrades. Networking is only instantiated when its installer is used; importing the module does not start a server or connect a client.
