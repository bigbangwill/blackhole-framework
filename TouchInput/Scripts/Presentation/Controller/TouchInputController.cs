using Blackhole.TouchInput.Application;
using UnityEngine;

namespace Blackhole.TouchInput.Presentation
{
    public class TouchInputController : IInputDirectionPort
    {
        public Vector2 GetInputDirection()
        {
            return new(1, 1);
        }
    }
}