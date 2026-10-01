using Blackhole.SharedKernel.Application;
using Blackhole.TouchInput.Contract;
using UnityEngine;

namespace Blackhole.TouchInput.Application
{
    public class InputStateProvider : IInputStateProvider, IApplicationTick
    {
        private readonly IInputDirectionPort inputDirectionPort;

        private InputStateDto state;

        public InputStateProvider(IInputDirectionPort inputDirectionPort)
        {
            this.inputDirectionPort = inputDirectionPort;
            state = new InputStateDto();
        }

        public InputStateDto GetInputState()
        {
            return state;
        }

        public void Tick(float deltaTime)
        {
            Vector2 direction = inputDirectionPort.GetInputDirection();
            state.input = direction;
        }
    }
}