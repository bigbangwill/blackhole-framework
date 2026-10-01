using Newtonsoft.Json;

namespace Blackhole.Authentication.Infrastructure
{
    public class AuthTokenApiResponse
    {
        [JsonProperty("access_token")]
        internal string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        internal string RefreshToken { get; set; }

        [JsonProperty("token_type")]
        internal string TokenType { get; set; }

        [JsonProperty("expires_in")]
        internal int ExpiresIn { get; set; }
    }
}