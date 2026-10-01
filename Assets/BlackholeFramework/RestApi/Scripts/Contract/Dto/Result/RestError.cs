namespace Blackhole.RestApi.Contract
{
    public sealed class RestError
    {
        public RestFailureKind Kind { get; }
        public string Code { get; }
        public string Message { get; }

        public RestError(RestFailureKind kind, string code, string message)
        {
            Kind = kind;
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Code)
                ? $"{Kind}: {Message}"
                : $"{Kind} [{Code}]: {Message}";
        }
    }
}