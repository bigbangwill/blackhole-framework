using Blackhole.ModuleSystem.Composition;

namespace Blackhole.JoinTest.Composition
{
    public sealed class JoinTestEntryPoint : RootModuleStartup
    {
        public JoinTestEntryPoint(RootModuleStartupRegistry registry) : base(registry)
        {
        }
    }
}
