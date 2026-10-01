namespace Blackhole.SharedKernel.Application
{
    public interface IApplicationTick
    {
        public void Tick(float deltaTime);
    }
}