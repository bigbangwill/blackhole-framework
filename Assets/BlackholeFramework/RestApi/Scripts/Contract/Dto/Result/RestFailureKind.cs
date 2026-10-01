namespace Blackhole.RestApi.Contract
{
    public enum RestFailureKind
    {
        Connection,
        Client,
        Server,
        Protocol,
        Unauthorized,
        Forbidden,
        DataProcessing,
        Serialization,
        RefreshFailed,
        Unknown
    }
}