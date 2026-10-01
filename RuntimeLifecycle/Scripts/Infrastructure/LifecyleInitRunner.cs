using Blackhole.SharedKernel.Application;
using System.Collections.Generic;
using VContainer.Internal;
using VContainer.Unity;

namespace Blackhole.RuntimeLifecycle.Infrastructure
{
    public class LifecyleInitRunner : IInitializable
    {
        private readonly IReadOnlyList<IApplicationInit> applicationInits;

        public LifecyleInitRunner(ContainerLocal<IReadOnlyList<IApplicationInit>> applicationInits)
        {
            this.applicationInits = applicationInits.Value;
        }

        public void Initialize()
        {
            foreach (var initable in applicationInits)
            {
                initable.Init();
            }
        }
    }
}