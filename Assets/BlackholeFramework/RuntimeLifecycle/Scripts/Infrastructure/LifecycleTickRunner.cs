using Blackhole.SharedKernel.Application;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Internal;
using VContainer.Unity;

namespace Blackhole.RuntimeLifecycle.Infrastructure
{
    public class LifecycleTickRunner : ITickable
    {
        private readonly IReadOnlyList<IApplicationTick> tickables;

        public LifecycleTickRunner(ContainerLocal<IReadOnlyList<IApplicationTick>> tickables)
        {
            this.tickables = tickables.Value;
        }

        public void Tick()
        {
            foreach (var tickable in tickables)
            {
                tickable.Tick(Time.deltaTime);
            }
        }
    }
}