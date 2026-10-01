using Cysharp.Threading.Tasks;
using System.Threading;
using Blackhole.Authentication.Contract;
using Blackhole.SharedKernel.Contract;

namespace Blackhole.Authentication.Application
{
    public interface IAuthenticationGateway
    {
        UniTask<Result<SessionTokenDto, RefreshTokenError>> RefreshToken(
            RefreshTokenCommand command,
            CancellationToken ct);

        UniTask<Result<SendSmsError>> SendSmsAsync(
            SendSmsCommand command,
            CancellationToken ct);

        UniTask<Result<SessionTokenDto, VerifySmsError>> VerifyCodeAsync(
            VerifySmsCommand command,
            CancellationToken ct);

        UniTask<Result<IsValidUserError>> IsValidUserAsync(
            IsValidUserCommand command,
            CancellationToken ct);
    }
}
