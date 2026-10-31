using Blackhole.Messaging.Contract;
using Blackhole.Networking.Application;
using Blackhole.Networking.Contract;
using Blackhole.SharedKernel.Application;
using Blackhole.SharedKernel.Contract;
using System;
using Unity.Collections;
using Unity.Networking.Transport;

namespace Blackhole.Networking.Infrastructure.UTP
{
    public class UtpClientRuntime : INetworkClient, IApplicationInit, IApplicationTick, IDisposable
    {
        private readonly IEventPublisher<NetworkConnectionStateChanged> _statePublisher;
        private readonly INetworkClientPayloadHandler _payloadHandler;

        private NetworkDriver _driver;
        private NetworkPipeline _reliablePipeline;
        private NetworkConnection _connection;

        private NetworkConnectionState _state = NetworkConnectionState.Disconnected;

        private bool _disposed;

        public NetworkConnectionState State => _state;

        public UtpClientRuntime(IEventPublisher<NetworkConnectionStateChanged> statePublisher,
            INetworkClientPayloadHandler payloadHandler)
        {
            _statePublisher = statePublisher;
            _payloadHandler = payloadHandler;
        }

        public void Init()
        {
            if (_disposed || _driver.IsCreated)
                return;

            _driver = NetworkDriver.Create();
            _reliablePipeline = _driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        }

        public void Tick(float deltaTime)
        {
            if (_disposed || !_driver.IsCreated)
                return;

            _driver.ScheduleUpdate().Complete();

            if (!_driver.IsCreated)
                return;

            NetworkEvent.Type networkEvent;

            while ((networkEvent = _connection.PopEvent(_driver, out DataStreamReader reader)) != NetworkEvent.Type.Empty)
            {
                if (networkEvent == NetworkEvent.Type.Connect)
                {
                    SetState(NetworkConnectionState.Connected);
                }
                else if(networkEvent == NetworkEvent.Type.Disconnect)
                {
                    _connection = default;
                    SetState(NetworkConnectionState.Disconnected);
                }
                else if (networkEvent == NetworkEvent.Type.Data)
                {
                    var bytes = new byte[reader.Length];

                    reader.ReadBytes(bytes.AsSpan());

                    if (!reader.HasFailedReads)
                    {
                        _payloadHandler.Handle(new(bytes));
                    }
                }
            }
        }

        private void SetState(NetworkConnectionState state)
        {
            if (_state == state)
                return;

            var previousState = _state;
            _state = state;

            _statePublisher.Publish(new(previousState, state));
        }

        public Result Connect(NetworkAddress address)
        {
            if(_disposed || !_driver.IsCreated || _state != NetworkConnectionState.Disconnected)
                return Result.Failure();

            if(string.IsNullOrEmpty(address.Host) || address.Port == 0)
                return Result.Failure();

            if(!NetworkEndpoint.TryParse(address.Host, address.Port, out var endpoint))
                return Result.Failure();

            _connection = _driver.Connect(endpoint);

            if (!_connection.IsCreated)
                return Result.Failure();

            SetState(NetworkConnectionState.Connecting);
            return Result.Success();

        }

        public Result Disconnect()
        {
            if (_disposed || !_driver.IsCreated)
                return Result.Failure();

            if(!_connection.IsCreated)
                return Result.Success();

            _connection.Disconnect(_driver);
            _driver.ScheduleUpdate().Complete();

            _connection = default;
            SetState(NetworkConnectionState.Disconnected);
            return Result.Success();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_driver.IsCreated)
            {
                Disconnect();
                _driver.Dispose();
            }

            _disposed = true;
        }

        public Result Send(NetworkPayload payload)
        {
            if (_state != NetworkConnectionState.Connected) 
                return Result.Failure();

            var result = _driver.BeginSend(_reliablePipeline, _connection, out var writer, payload.Data.Length);

            if (result != 0)
                return Result.Failure();

            var span = payload.Data.Span;

            for (int i = 0; i < span.Length; i++)
            {
                writer.WriteByte(span[i]);
            }

            if (writer.HasFailedWrites)
            {
                _driver.AbortSend(writer);
                return Result.Failure();
            }

            var bytesSent = _driver.EndSend(writer);

            if (bytesSent < 0)
            {
                return Result.Failure();
            }

            return Result.Success();
        }

    }
}