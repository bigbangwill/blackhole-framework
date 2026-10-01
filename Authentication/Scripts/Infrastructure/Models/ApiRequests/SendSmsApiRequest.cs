using Newtonsoft.Json;

namespace Blackhole.Authentication.Infrastructure
{
    internal readonly struct SendSmsApiRequest
    {
        [JsonProperty("mobileNumber")]
        internal readonly string MobileNumber;

        [JsonProperty("countryId")]
        internal readonly int CountryId;

        [JsonProperty("platformName")]
        internal readonly string PlatformName;

        internal SendSmsApiRequest(string mobileNumber, int countryId = 411, string platformName = null)
        {
            MobileNumber = mobileNumber;
            CountryId = countryId;
            PlatformName = platformName;
        }
    }
}