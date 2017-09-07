using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez;

namespace EyesHaveIt.Utilities
{
    class GrayscalePostProcessor : PostProcessor<GrayscaleEffect>
    {
        public GrayscalePostProcessor(int executionOrder) : base(executionOrder)
        {
            effect = new GrayscaleEffect();
        }
    }
}
