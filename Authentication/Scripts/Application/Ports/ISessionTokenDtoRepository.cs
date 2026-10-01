using Blackhole.SharedKernel.Contract;

namespace Blackhole.Authentication.Application
{
    public interface ISessionTokenDtoRepository
    {
        Result<SessionTokenDto, SessionTokenError> GetSessionTokenDto();
        void SetSessionTokenDto(SessionTokenDto sessionTokenDto);
        void ClearSessionTokenDto();
    }
}
