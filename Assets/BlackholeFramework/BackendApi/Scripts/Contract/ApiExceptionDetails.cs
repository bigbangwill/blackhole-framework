using Newtonsoft.Json;

namespace Blackhole.BackendApi.Contract
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ApiExceptionDetails
    {
        [JsonProperty("code")]
        public string Code { get; }

        [JsonProperty("persianDescription")]
        public string PersianDescription { get; }

        [JsonProperty("englishDescription")]
        public string EnglishDescription { get; }

        [JsonConstructor]
        public ApiExceptionDetails(string code, string persianDescription, string englishDescription)
        {
            Code = code ?? string.Empty;
            PersianDescription = persianDescription ?? string.Empty;
            EnglishDescription = englishDescription ?? string.Empty;
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Code)
                ? EnglishDescription
                : $"[{Code}] {EnglishDescription}";
        }
    }
}
