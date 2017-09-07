using Nez;

namespace EyesHaveIt.Utilities
{
    class NoiseEffectPostProcessor : PostProcessor<NoiseEffect>
    {
        public NoiseEffectPostProcessor(int executionOrder) : base(executionOrder)
        {
            effect = new NoiseEffect();
        }
    }
}
