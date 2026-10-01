using Cysharp.Threading.Tasks;
using Blackhole.LoadingScreen.Contract; 

namespace Blackhole.LoadingScreen.Application
{
    public interface ILoadingScreenPresenter
    {
        UniTask FadeIn(LoadingScreenType type);
        UniTask FadeOut(LoadingScreenType type);
    }
}