using Nez;

namespace EyesHaveIt.Utilities
{
    internal class NoiseEffectPostProcessor : PostProcessor<NoiseEffect>
    {
        public NoiseEffectPostProcessor(int executionOrder) : base(executionOrder)
        {
            effect = new NoiseEffect();
        }
    }
}
