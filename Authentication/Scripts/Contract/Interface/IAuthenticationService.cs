using System.Threading;
using Blackhole.SharedKernel.Contract;
using Cysharp.Threading.Tasks;

namespace Blackhole.Authentication.Contract
{
    public interface IAuthenticationService
    {
        void Init();

        UniTask<Result<SendSmsError>> SendSmsAsync(SendSmsRequestDto request, CancellationToken ct);

        UniTask<Result<VerifySmsError>> VerifyCodeAsync(VerifySmsRequestDto request, CancellationToken ct);

        UniTask<Result<IsValidUserError>> IsValidUserAsync(CancellationToken ct);
    }
}
