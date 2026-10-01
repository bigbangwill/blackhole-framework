using Blackhole.Authentication.Contract;
using Blackhole.Authentication.Domain;
using Blackhole.SharedKernel.Contract;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Blackhole.LocalStorageFile.Contract;

namespace Blackhole.Authentication.Application
{
    public class AuthenticationService : IAuthenticationService
    {
        private SessionToken _sessionToken;
        private IAuthenticationGateway _authenticationGateway;
        private ISessionTokenDtoRepository _sessionTokenDtoRepository;
        private ISaveData _saveData;
        private readonly IDataSerializer _dataSerializer;
        private const string SessionTokenStorageKey = "SessionTokenStorageKey";

        public AuthenticationService( IAuthenticationGateway authenticationGateway, ISessionTokenDtoRepository sessionTokenDtoRepository, ISaveData saveData, IDataSerializer dataSerializer)
        {
            _authenticationGateway = authenticationGateway;
            _sessionTokenDtoRepository = sessionTokenDtoRepository;
            _saveData = saveData;
            _dataSerializer = dataSerializer;
        }

        public UniTask<Result<SendSmsError>> SendSmsAsync(
            SendSmsRequestDto request,
            CancellationToken ct)
        {
            var command = new SendSmsCommand(request.MobileNumber);
            return _authenticationGateway.SendSmsAsync(command, ct);
        }

        public async UniTask<Result<VerifySmsError>> VerifyCodeAsync(
            VerifySmsRequestDto request,
            CancellationToken ct)
        {
            var command = new VerifySmsCommand(request.MobileNumber, request.VerifyCode);
            var result = await _authenticationGateway.VerifyCodeAsync(command, ct);

            if (result.IsFailure)
            {
                return Result<VerifySmsError>.Failure(result.Error);
            }

            SetSessionToken(result.Value);
            return Result<VerifySmsError>.Success();
        }

        public Result<KeyValuePair<string, string>, SessionTokenError> GetAccessTokenHeader()
        {
            if (_sessionToken == null)
            {
                return Result<KeyValuePair<string, string>, SessionTokenError>
                    .Failure(SessionTokenError.NotFound);
            }

            return Result<KeyValuePair<string, string>, SessionTokenError>
                .Success(_sessionToken.AccessTokenHeader());
        }

        public async UniTask<Result<SessionTokenDto, RefreshTokenError>> RefreshToken(CancellationToken ct)
        {
            if (_sessionToken == null)
            {
                return Result<SessionTokenDto, RefreshTokenError>
                    .Failure(RefreshTokenError.MissingSessionToken);
            }

            var command = new RefreshTokenCommand(_sessionToken.RefreshTokenHeader());

            var result = await _authenticationGateway.RefreshToken(command, ct);
            if (result.IsSuccess)
            {
                SetSessionToken(result.Value);
            }

            return result;
        }

        public async UniTask<Result<IsValidUserError>> IsValidUserAsync(CancellationToken ct)
        {
            if (_sessionToken == null)
            {
                return Result<IsValidUserError>.Failure(IsValidUserError.InvalidToken);
            }

            var header = _sessionToken.AccessTokenHeader();
            var command = new IsValidUserCommand(header);
            var result = await _authenticationGateway.IsValidUserAsync(command, ct);

            if (result.IsSuccess)
            {
                return Result<IsValidUserError>.Success();
            }

            var refresh = await RefreshToken(ct);
            if (refresh.IsFailure)
            {
                return Result<IsValidUserError>.Failure(result.Error);
            }

            var retryCommand = new IsValidUserCommand(_sessionToken.AccessTokenHeader());
            var retryResult = await _authenticationGateway.IsValidUserAsync(retryCommand, ct);

            return retryResult.IsSuccess
                ? Result<IsValidUserError>.Success()
                : Result<IsValidUserError>.Failure(retryResult.Error);
        }

        public void Init()
        {
            var load = _saveData.LoadData(SessionTokenStorageKey);
            if (load.IsFailure)
            {
                Debug.Log("No persisted authentication session was found.");
                return;
            }

            var deserialize = _dataSerializer.DeserializeData<SessionTokenDto>(load.Value);
            if (deserialize.IsFailure)
            {
                Debug.LogError($"Failed to deserialize session token: {deserialize.Error}");
                return;
            }

            var tokenDto = deserialize.Value;
            _sessionToken = new SessionToken(tokenDto.AccessToken, tokenDto.RefreshToken, tokenDto.TokenType, tokenDto.ExpiresIn);
        }

        //public void Init()
        //{
        //    var result = _sessionTokenDtoRepository.GetSessionTokenDto();
        //    if (result.IsFailure)
        //    {
        //        Debug.Log("No persisted authentication session was found.");
        //        return;
        //    }

        //    SessionTokenDto tokenDto = result.Value;
        //    _sessionToken = new(tokenDto.AccessToken, tokenDto.RefreshToken, tokenDto.TokenType, tokenDto.ExpiresIn);

        //    Debug.Log(JsonConvert.SerializeObject(tokenDto, Formatting.Indented));
        //}

        private void SetSessionToken(SessionTokenDto tokenDto)
        {
            _sessionTokenDtoRepository.SetSessionTokenDto(tokenDto);

            // Persist raw serialized form
            var serializeResult = _dataSerializer.SerializeData(tokenDto);
            if (serializeResult.IsSuccess)
            {
                var saveResult = _saveData.SaveData(SessionTokenStorageKey, serializeResult.Value);
                if (saveResult.IsFailure)
                {
                    Debug.LogError("Failed to save session token to storage.");
                }
            }
            else
            {
                Debug.LogError($"Failed to serialize session token: {serializeResult.Error}");
            }

            if (_sessionToken == null)
            {
                _sessionToken = new SessionToken(
                    tokenDto.AccessToken,
                    tokenDto.RefreshToken,
                    tokenDto.TokenType,
                    tokenDto.ExpiresIn);
                return;
            }

            _sessionToken.Replace(
                tokenDto.AccessToken,
                tokenDto.RefreshToken,
                tokenDto.TokenType,
                tokenDto.ExpiresIn);
        }
    }
}
