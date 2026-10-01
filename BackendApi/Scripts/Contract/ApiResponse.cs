using System;
using Newtonsoft.Json;

namespace Blackhole.BackendApi.Contract
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ApiResponse
    {
        [JsonProperty("id")]
        public long Id { get; }

        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; }

        [JsonProperty("exceptions")]
        public ApiExceptionEntry[] Exceptions { get; }

        public bool HasExceptions => Exceptions.Length > 0;

        [JsonConstructor]
        public ApiResponse(long id, bool isSuccess, ApiExceptionEntry[] exceptions)
        {
            Id = id;
            IsSuccess = isSuccess;
            Exceptions = exceptions ?? Array.Empty<ApiExceptionEntry>();
        }
    }
}
