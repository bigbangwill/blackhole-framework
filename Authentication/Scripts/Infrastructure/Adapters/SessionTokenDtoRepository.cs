using Blackhole.Authentication.Application;
using Blackhole.SharedKernel.Contract;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Blackhole.Authentication.Infrastructure
{
    public class SessionTokenDtoRepository : ISessionTokenDtoRepository
    {
        private readonly SessionTokenSO _sessionTokenSO;

        public SessionTokenDtoRepository(SessionTokenSO sessionTokenSO)
        {
            _sessionTokenSO = sessionTokenSO;
        }

        public Result<SessionTokenDto, SessionTokenError> GetSessionTokenDto()
        {
            if (string.IsNullOrEmpty(_sessionTokenSO.AccessToken) ||
                string.IsNullOrEmpty(_sessionTokenSO.RefreshToken))
            {
                return Result<SessionTokenDto, SessionTokenError>.Failure(SessionTokenError.NotFound);
            }

            var sessionTokenDto = new SessionTokenDto(
                _sessionTokenSO.AccessToken,
                _sessionTokenSO.RefreshToken,
                _sessionTokenSO.TokenType,
                _sessionTokenSO.ExpiresIn);

            return Result<SessionTokenDto, SessionTokenError>.Success(sessionTokenDto);
        }

        public void SetSessionTokenDto(SessionTokenDto sessionTokenDto)
        {
            _sessionTokenSO.AccessToken = sessionTokenDto.AccessToken;
            _sessionTokenSO.RefreshToken = sessionTokenDto.RefreshToken;
            _sessionTokenSO.TokenType = sessionTokenDto.TokenType;
            _sessionTokenSO.ExpiresIn = sessionTokenDto.ExpiresIn;

#if UNITY_EDITOR
            EditorUtility.SetDirty(_sessionTokenSO);
#endif
        }

        public void ClearSessionTokenDto()
        {
            throw new System.NotImplementedException();
        }
    }
}
