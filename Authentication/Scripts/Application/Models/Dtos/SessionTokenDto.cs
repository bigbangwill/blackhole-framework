namespace Blackhole.Authentication.Application
{
    public class SessionTokenDto
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }
        public string TokenType { get; }
        public int ExpiresIn { get; }

        public SessionTokenDto(string accessToken, string refreshToken, string tokenType, int expiresIn)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            TokenType = tokenType;
            ExpiresIn = expiresIn;
        }
    }
}