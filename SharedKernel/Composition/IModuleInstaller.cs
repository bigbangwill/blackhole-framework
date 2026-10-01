using VContainer;

namespace Blackhole.SharedKernel.Composition
{
    public interface IModuleInstaller
    {
        public void InstallModule(IContainerBuilder builder);
    }
}