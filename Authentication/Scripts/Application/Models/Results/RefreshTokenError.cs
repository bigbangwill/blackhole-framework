namespace Blackhole.Authentication.Application
{
    public enum RefreshTokenError
    {
        ExpiredRefresh,
        Unauthorized,
        MissingSessionToken,
        Unknown
    }
}
