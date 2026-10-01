using System.Collections.Generic;

namespace Blackhole.Authentication.Application
{
    public class RefreshTokenCommand
    {
        public KeyValuePair<string, string> RefreshToken { get; }

        public RefreshTokenCommand(KeyValuePair<string, string> refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}