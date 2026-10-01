using Blackhole.Authentication.Application;
using Blackhole.Authentication.Contract;
using Blackhole.BackendApi.Contract;
using Blackhole.RestApi.Contract;
using Blackhole.SharedKernel.Contract;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;

namespace Blackhole.Authentication.Infrastructure
{
    public class RestAuthenticationGateway : IAuthenticationGateway
    {
        private readonly IRestApiClient _restApiClient;
        private readonly AuthenticationSettingSO _authenticationSettingSO;

        public RestAuthenticationGateway(IRestApiClient restApiClient, AuthenticationSettingSO authenticationSettingSO)
        {
            _restApiClient = restApiClient;
            _authenticationSettingSO = authenticationSettingSO;
        }

        public async UniTask<Result<SessionTokenDto, RefreshTokenError>> RefreshToken(RefreshTokenCommand command, CancellationToken ct)
        {
            Debug.Log($"Refreshing token with refresh token: {command.RefreshToken.Value}");
            string endpoint = "/api/Principal/Refresh";

            Dictionary<string, string> headers = new()
            {
                { "Content-Type", "application/x-www-form-urlencoded" },
                { command.RefreshToken.Key, command.RefreshToken.Value }
            };

            var restRequest = RestRequest.Post(endpoint, headers: headers);
            restRequest.SetBaseUrl(_authenticationSettingSO.BaseUrl);

            var result = await _restApiClient.Send(restRequest, ct);

            if (result.IsSuccess)
            {
                var authTokenApiResponse = JsonConvert.DeserializeObject<AuthTokenApiResponse>(result.ResponseMessage);
                return Result<SessionTokenDto, RefreshTokenError>
                    .Success(authTokenApiResponse.ToSessionToken());
            }
            else
            {
                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return Result<SessionTokenDto, RefreshTokenError>
                        .Failure(RefreshTokenError.Unauthorized);
                }
                else
                {
                    Debug.LogError($"Couldnt refresh. Error: {result.Error}");
                    return Result<SessionTokenDto, RefreshTokenError>
                        .Failure(RefreshTokenError.Unknown);
                }
            }
        }

        public async UniTask<Result<SessionTokenDto, VerifySmsError>> VerifyCodeAsync(VerifySmsCommand command, CancellationToken ct)
        {
            string endPoint = "/api/Principal/VerifyValidationCode";

            var request = new VerifySmsApiRequest(command.VerifyCode, command.MobileNumber);

            Dictionary<string, string> headers = new()
            {
                {"Content-Type","application/x-www-form-urlencoded" },
                {"VerifyCode",request.VerifyCode },
                {"MobileNumber",request.MobileNumber },
                {"IdentityNo",request.IdentityNo },
                {"CountryId",request.CountryId }
            };

            var restRequest = RestRequest.Post(endPoint, headers: headers);
            restRequest.SetBaseUrl(_authenticationSettingSO.BaseUrl);

            var result = await _restApiClient.Send(restRequest, ct);

            if (result.IsSuccess)
            {
                var authTokenResponse = JsonConvert.DeserializeObject<AuthTokenApiResponse>(result.ResponseMessage);
                return Result<SessionTokenDto, VerifySmsError>
                    .Success(authTokenResponse.ToSessionToken());
            }
            else
            {
                if(result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return Result<SessionTokenDto, VerifySmsError>
                        .Failure(VerifySmsError.WrongCode);
                }
                else
                {
                    Debug.LogError($"Couldnt verify code. Error: {result.Error}");
                    return Result<SessionTokenDto, VerifySmsError>
                        .Failure(VerifySmsError.Unknown);
                }
            }
        }

        public async UniTask<Result<SendSmsError>> SendSmsAsync(SendSmsCommand command, CancellationToken ct)
        {
            string endPoint = "/api/Principal/GenerateUserValidationCode";

            var request = new SendSmsApiRequest(command.MobileNumber);

            Dictionary<string, string> headers = new()
            {
                {"Content-Type","application/json" }
            };

            var jsonBody = JsonConvert.SerializeObject(request);

            var restRequest = RestRequest.Post(endPoint, jsonBody, headers);
            restRequest.SetBaseUrl(_authenticationSettingSO.BaseUrl);

            var result = await _restApiClient.Send<ApiResponse>(restRequest, ct);

            if (result.IsSuccess)
            {
                var apiResponse = result.Value;
                if (apiResponse.IsSuccess)
                {
                    return Result<SendSmsError>.Success();
                }
                else
                {
                    long errorCode = long.Parse(apiResponse.Exceptions[0].Exception.Code);
                    return errorCode switch
                    {
                        202 => Result<SendSmsError>.Failure(SendSmsError.
                        
                        CodeExists),
                        236 => Result<SendSmsError>.Failure(SendSmsError.WrongNumber),
                        _ => Result<SendSmsError>.Failure(SendSmsError.Unknown),
                    };
                }
            }
            else
            {
                Debug.LogError($"Couldnt send sms. Error: {result.Error}");
                return Result<SendSmsError>.Failure(SendSmsError.Unknown);
            }
        }

        public async UniTask<Result<IsValidUserError>> IsValidUserAsync(IsValidUserCommand command, CancellationToken ct)
        {
            string endPoint = "/api/Principal/IsValidUser";
            var headers = new Dictionary<string, string>
            {
                { command.AccessTokenHeader.Key, command.AccessTokenHeader.Value }
            };

            var restRequest = RestRequest.Post(endPoint, "", headers);

            restRequest.SetBaseUrl(_authenticationSettingSO.BaseUrl);

            var result = await _restApiClient.Send<ApiResponse>(restRequest, ct);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return Result<IsValidUserError>.Success();
            }

            return result.StatusCode switch
            {
                HttpStatusCode.Unauthorized => Result<IsValidUserError>.Failure(IsValidUserError.ExpiredToken),
                HttpStatusCode.Forbidden => Result<IsValidUserError>.Failure(IsValidUserError.InvalidToken),
                HttpStatusCode.NotFound => Result<IsValidUserError>.Failure(IsValidUserError.UserNotFound),
                _ => Result<IsValidUserError>.Failure(IsValidUserError.Unknown)
            };
        }
    }
}
