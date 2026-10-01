using System;
using System.Net;
using System.Threading;
using Blackhole.RestApi.Contract;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Blackhole.RestApi.Infrastructure
{
    public sealed class RestApiClient : IRestApiClient
    {
        public async UniTask<RestResult> Send(RestRequest request, CancellationToken ct)
        {
            RestResult<string> rawResult = await SendRaw(request, ct);

            if (rawResult.IsFailure)
                return RestResult.Failure(rawResult.Error, rawResult.StatusCode, rawResult.ResponseMessage);

            return RestResult.Success(rawResult.StatusCode ?? HttpStatusCode.OK, rawResult.ResponseMessage);
        }

        public async UniTask<RestResult<T>> Send<T>(RestRequest request, CancellationToken ct)
        {
            RestResult<string> rawResult = await SendRaw(request, ct);

            if (rawResult.IsFailure)
                return RestResult<T>.Failure(rawResult.Error, rawResult.StatusCode, rawResult.ResponseMessage);

            HttpStatusCode statusCode = rawResult.StatusCode ?? HttpStatusCode.OK;
            string responseBody = rawResult.Value;

            if (typeof(T) == typeof(string))
                return RestResult<T>.Success((T)(object)responseBody, statusCode, responseBody);

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                var error = new RestError(RestFailureKind.Serialization, ((int)statusCode).ToString(), $"The server returned an empty response while a value of type '{typeof(T).Name}' was expected.");
                return RestResult<T>.Failure(error, statusCode, responseBody);
            }

            try
            {
                T value = JsonConvert.DeserializeObject<T>(responseBody);

                if (value is null)
                {
                    var error = new RestError(RestFailureKind.Serialization, ((int)statusCode).ToString(), $"The server response was deserialized to null for type '{typeof(T).Name}'.");
                    return RestResult<T>.Failure(error, statusCode, responseBody);
                }

                return RestResult<T>.Success(value, statusCode, responseBody);
            }
            catch (Exception exception)
            {
                RestError error = RestErrorFactory.Serialization(exception);
                return RestResult<T>.Failure(error, statusCode, responseBody);
            }
        }

        private async UniTask<RestResult<string>> SendRaw(RestRequest request, CancellationToken ct)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            using UnityWebRequest webRequest = UnityWebRequestFactory.Create(request, request.FullUrl);

            try
            {
                await webRequest.SendWebRequest().ToUniTask(cancellationToken: ct);

                string responseBody = webRequest.downloadHandler?.text ?? string.Empty;

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    RestError requestError = RestErrorFactory.FromRequest(webRequest);
                    return RestResult<string>.Failure(requestError, GetStatusCode(webRequest.responseCode), responseBody);
                }

                HttpStatusCode statusCode = GetStatusCode(webRequest.responseCode) ?? HttpStatusCode.OK;
                return RestResult<string>.Success(responseBody, statusCode, responseBody);
            }
            catch (OperationCanceledException)
            {
                Debug.LogError("MOTHER FUCKER");
                throw;
            }
            catch (Exception exception)
            {
                RestError requestError = RestErrorFactory.FromRequest(webRequest, exception);
                string responseBody = webRequest.downloadHandler?.text ?? string.Empty;
                return RestResult<string>.Failure(requestError, GetStatusCode(webRequest.responseCode), responseBody);
            }
        }

        private static HttpStatusCode? GetStatusCode(long responseCode)
        {
            if (responseCode < 100 || responseCode > 599)
            {
                Debug.Log(responseCode + " Got this code");
                return null;
            }

            return (HttpStatusCode)responseCode;
        }
    }
}

