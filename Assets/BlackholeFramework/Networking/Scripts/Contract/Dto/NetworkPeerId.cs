namespace Blackhole.Networking.Contract
{
    public readonly struct NetworkPeerId
    {
        public int Id { get; }
        public NetworkPeerId(int id)
        {
            Id = id;
        }

        public bool Equals(NetworkPeerId other) => Id == other.Id;
        public override bool Equals(object obj) => obj is NetworkPeerId other && Equals(other);

        public static bool operator ==(NetworkPeerId left, NetworkPeerId right) => left.Equals(right);
        public static bool operator !=(NetworkPeerId left, NetworkPeerId right) => !left.Equals(right);

        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString() => Id.ToString();
    }
}