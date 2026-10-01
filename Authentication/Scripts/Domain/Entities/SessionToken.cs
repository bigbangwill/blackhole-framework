using System;
using System.Collections.Generic;

namespace Blackhole.Authentication.Domain
{
    public class SessionToken
    {
        private string _accessToken;
        private string _refreshToken;
        private string _tokenType;
        private DateTime _expiresIn;

        public SessionToken(string accessToken, string refreshToken, string tokenType, int expireTimer)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _tokenType = tokenType;
            _expiresIn = DateTime.UtcNow.AddSeconds(expireTimer);
        }

        public KeyValuePair<string, string> AccessTokenHeader() => new("Authorization", $"{_tokenType} {_accessToken}");

        public KeyValuePair<string, string> RefreshTokenHeader() => new("RefreshToken", _refreshToken);

        public bool IsExpired() => _expiresIn < DateTime.UtcNow;

        public void Replace(string accessToken, string refreshToken, string tokenType, int expireTimer)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _tokenType = tokenType;
            _expiresIn = DateTime.UtcNow.AddSeconds(expireTimer);
        }
    }
}