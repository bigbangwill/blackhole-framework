using System;
using Blackhole.RestApi.Contract;
using UnityEngine.Networking;

namespace Blackhole.RestApi.Infrastructure
{
    internal static class RestErrorFactory
    {
        public static RestError FromRequest(UnityWebRequest request, Exception exception = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string code = request.responseCode > 0
                ? request.responseCode.ToString()
                : string.Empty;

            return request.responseCode switch
            {
                401 => new RestError(
                    RestFailureKind.Unauthorized,
                    code,
                    request.error ?? "Authentication is required or the access token is invalid."),

                403 => new RestError(
                    RestFailureKind.Forbidden,
                    code,
                    request.error ?? "The authenticated user is not allowed to perform this operation."),

                >= 400 and < 500 => new RestError(
                    RestFailureKind.Client,
                    code,
                    CreateProtocolMessage(request)),

                >= 500 and < 600 => new RestError(
                    RestFailureKind.Server,
                    code,
                    CreateProtocolMessage(request)),

                _ => CreateNonHttpError(request, code, exception)
            };
        }

        public static RestError Serialization(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return new RestError(
                RestFailureKind.Serialization,
                exception.GetType().Name,
                $"The server response could not be deserialized. {exception.Message}");
        }

        private static RestError CreateNonHttpError(UnityWebRequest request, string code, Exception exception)
        {
            return request.result switch
            {
                UnityWebRequest.Result.ConnectionError => new RestError(
                    RestFailureKind.Connection,
                    code,
                    request.error ?? "The server could not be reached."),

                UnityWebRequest.Result.DataProcessingError => new RestError(
                    RestFailureKind.DataProcessing,
                    code,
                    request.error ?? "The response data could not be processed."),

                UnityWebRequest.Result.ProtocolError => new RestError(
                    RestFailureKind.Protocol,
                    code,
                    CreateProtocolMessage(request)),

                _ => new RestError(
                    RestFailureKind.Unknown,
                    code,
                    exception?.Message ?? request.error ?? "An unexpected REST request error occurred.")
            };
        }

        private static string CreateProtocolMessage(UnityWebRequest request)
        {
            if (request.responseCode > 0)
                return $"The server returned HTTP status code {request.responseCode}.";

            return request.error ?? "The server rejected the request.";
        }
    }
}