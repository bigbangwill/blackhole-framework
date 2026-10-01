using System.Threading;
using Cysharp.Threading.Tasks;
using Blackhole.RestApi.Contract;
using Blackhole.Authentication.Contract;
using Blackhole.Authentication.Application;
using System.Collections.Generic;
using System.Net;
using Blackhole.SharedKernel.Contract;
using UnityEngine;

namespace Blackhole.Authentication.Infrastructure
{
    public class AuthenticatedRestApiClient : IAuthenticatedRestApiClient
    {
        private readonly IRestApiClient _restApiClient;
        private readonly AuthenticationService _authenticationService;

        public AuthenticatedRestApiClient(IRestApiClient restApiClient, AuthenticationService authenticationService)
        {
            _restApiClient = restApiClient;
            _authenticationService = authenticationService;
        }

        public async UniTask<RestResult> Send(RestRequest request, CancellationToken ct)
        {
            Result<KeyValuePair<string, string>, SessionTokenError> accessHeaderResult =
                _authenticationService.GetAccessTokenHeader();

            if (accessHeaderResult.IsFailure)
            {
                return RestResult.Failure(CreateMissingSessionTokenError(), HttpStatusCode.Unauthorized);
            }

            var accessHeader = accessHeaderResult.Value;
            request.Headers[accessHeader.Key] = accessHeader.Value;

            var result = await _restApiClient.Send(request, ct);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshResult = await _authenticationService.RefreshToken(ct);
                if (refreshResult.IsSuccess)
                {
                    var refreshedHeaderResult = _authenticationService.GetAccessTokenHeader();
                    if (refreshedHeaderResult.IsFailure)
                    {
                        return RestResult.Failure(CreateMissingSessionTokenError(), HttpStatusCode.Unauthorized);
                    }

                    var refreshedHeader = refreshedHeaderResult.Value;
                    request.Headers[refreshedHeader.Key] = refreshedHeader.Value;
                    var retryResult = await _restApiClient.Send(request, ct);
                    return retryResult;
                }

                RefreshTokenError refreshError = refreshResult.Error;
                RestError error = CreateRefreshError(refreshError);
                HttpStatusCode? statusCode = IsUnauthorized(refreshError)
                    ? HttpStatusCode.Unauthorized
                    : null;

                return RestResult.Failure(error, statusCode);
            }

            return result;
        }

        public async UniTask<RestResult<T>> Send<T>(RestRequest request, CancellationToken ct)
        {
            Result<KeyValuePair<string, string>, SessionTokenError> accessHeaderResult = _authenticationService.GetAccessTokenHeader();

            if (accessHeaderResult.IsFailure)
            {
                Debug.Log("FAILED TO GET ACCESS TOKEN HEADER");
                return RestResult<T>.Failure(CreateMissingSessionTokenError(), HttpStatusCode.Unauthorized);
            }

            var accessHeader = accessHeaderResult.Value;
            request.Headers[accessHeader.Key] = accessHeader.Value;

            var result = await _restApiClient.Send<T>(request, ct);

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshResult = await _authenticationService.RefreshToken(ct);
                if (refreshResult.IsSuccess)
                {
                    var refreshedHeaderResult = _authenticationService.GetAccessTokenHeader();
                    if (refreshedHeaderResult.IsFailure)
                    {
                        Debug.Log("FAILED INTERNALLY TO GET ACCESS TOKEN HEADER AFTER REFRESH");
                        return RestResult<T>.Failure(CreateMissingSessionTokenError(), HttpStatusCode.Unauthorized);
                    }

                    var refreshedHeader = refreshedHeaderResult.Value;
                    request.Headers[refreshedHeader.Key] = refreshedHeader.Value;
                    var retryResult = await _restApiClient.Send<T>(request, ct);
                    return retryResult;
                }

                RefreshTokenError refreshError = refreshResult.Error;
                RestError error = CreateRefreshError(refreshError);
                HttpStatusCode? statusCode = IsUnauthorized(refreshError)
                    ? HttpStatusCode.Unauthorized
                    : null;

                Debug.Log(error.Message);
                return RestResult<T>.Failure(error, statusCode);
            }

            return result;
        }

        private static RestError CreateMissingSessionTokenError()
        {
            return new RestError(
                RestFailureKind.Unauthorized,
                SessionTokenError.NotFound.ToString(),
                "An authenticated request requires a session token.");
        }

        private static RestError CreateRefreshError(RefreshTokenError error)
        {
            RestFailureKind kind = IsUnauthorized(error)
                ? RestFailureKind.Unauthorized
                : RestFailureKind.RefreshFailed;

            return new RestError(
                kind,
                error.ToString(),
                "The access token was unauthorized and the refresh attempt failed.");
        }

        private static bool IsUnauthorized(RefreshTokenError error)
        {
            return error == RefreshTokenError.Unauthorized ||
                   error == RefreshTokenError.MissingSessionToken;
        }
    }
}
