using System.Threading;
using Cysharp.Threading.Tasks;

namespace Blackhole.ModuleSystem.Contract
{
    public interface IModuleLoader
    {
        UniTask LoadRootModuleAsync(string rootModuleID, CancellationToken cancellationToken);
        UniTask LoadRootModuleAsync<TEntryOptions>(string rootModuleID, TEntryOptions entryOptions, CancellationToken cancellationToken) where TEntryOptions : class;
        UniTask UnloadCurrentRootModuleAsync(CancellationToken cancellationToken);
    }
}
