using System;

namespace Blackhole.Networking.Contract
{
    public readonly struct NetworkPayload
    {
        public ReadOnlyMemory<byte> Data { get; }

        public NetworkPayload(ReadOnlyMemory<byte> data)
        {
            Data = data;
        }
    }
}