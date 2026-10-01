namespace Blackhole.Authentication.Contract
{
    public enum IsValidUserError
    {
        InvalidToken,
        ExpiredToken,
        UserNotFound,
        Unknown
    }
}