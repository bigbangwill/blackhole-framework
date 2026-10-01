using Blackhole.Authentication.Application;

namespace Blackhole.Authentication.Infrastructure
{
    public static class SessionTokenMapper
    {
        public static SessionTokenDto ToSessionToken(this AuthTokenApiResponse authTokenApiResponse)
        {
            return new SessionTokenDto(authTokenApiResponse.AccessToken, authTokenApiResponse.RefreshToken, authTokenApiResponse.TokenType, authTokenApiResponse.ExpiresIn);
        }
    }
}