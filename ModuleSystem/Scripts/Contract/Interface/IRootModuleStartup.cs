using System.Threading;
using Cysharp.Threading.Tasks;

namespace Blackhole.ModuleSystem.Contract
{
    public interface IRootModuleStartup
    {
        UniTask InitializeAsync(CancellationToken cancellationToken);
    }
}
