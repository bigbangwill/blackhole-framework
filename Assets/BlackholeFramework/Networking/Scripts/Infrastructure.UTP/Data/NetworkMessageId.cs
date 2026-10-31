using Unity.Collections;

namespace Blackhole.Networking.Infrastructure.UTP
{
    internal enum NetworkMessageId : byte
    {
	    Ping = 1,
        Pong = 2
    }

    internal readonly struct PingMessage
    {
        public uint Sequence { get; }
        public PingMessage(uint sequence)
        {
            Sequence = sequence;
        }
    }

    internal readonly struct PongMessage
    {
        public uint Sequence { get; }

        public PongMessage(uint sequence)
        {
            Sequence = sequence;
        }
    }

    internal static class PongProtocol
    {
        public static void WritePong(ref DataStreamWriter writer, PongMessage message)
        {
            writer.WriteByte((byte)NetworkMessageId.Pong);
            writer.WriteUInt(message.Sequence);
        }

        public static bool TryReadPong(ref DataStreamReader reader, out PongMessage message)
        {
            message = default;

            uint sequence = reader.ReadUInt();

            if (reader.HasFailedReads)
                return false;

            message = new PongMessage(sequence);
            return true;
        }
    }

    internal static class PingProtocol
    {
        public static void WritePing(ref DataStreamWriter writer, PingMessage message)
        {
            writer.WriteByte((byte)NetworkMessageId.Ping);
            writer.WriteUInt(message.Sequence);
        }

        public static bool TryReadPing(ref DataStreamReader reader, out PingMessage message)
        {
            message = default;

            var sequence = reader.ReadUInt();

            if (reader.HasFailedReads)
                return false;

            message = new PingMessage(sequence);
            return true;
        }
    }
}