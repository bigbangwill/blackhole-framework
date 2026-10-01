using Newtonsoft.Json;

namespace Blackhole.BackendApi.Contract
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ApiExceptionEntry
    {
        [JsonProperty("exception")]
        public ApiExceptionDetails Exception { get; }

        [JsonConstructor]
        public ApiExceptionEntry(ApiExceptionDetails exception)
        {
            Exception = exception;
        }
    }
}
