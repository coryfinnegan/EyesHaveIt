using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;

namespace EyesHaveIt.Utilities
{
    class PostEffectController : Component, IUpdatable
    {
        Scene myScene;
        public GrayscalePostProcessor greyScalePostProcessor { get; set; }
        public PixelGlitchPostProcessor pixelGlitchPostProcessor { get; set; }
        public HeatDistortionPostProcessor heatDistortionPostProcessor { get; set; }
        ScanlinesPostProcessor scanLinesPostProcessor;
        NoiseEffectPostProcessor noisePostProcessor;

        public PostEffectController()
        {
        }
        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
        }
        public void addGreyScale()
        {
            greyScalePostProcessor = entity.scene.addPostProcessor(new GrayscalePostProcessor(0));
        }
        public void removeGreyScale()
        {
            entity.scene.removePostProcessor(greyScalePostProcessor);
        }
        public void addPixelGlitch()
        {
            pixelGlitchPostProcessor = entity.scene.addPostProcessor(new PixelGlitchPostProcessor(0));
        }
        public void removePixelGlitch()
        {
            entity.scene.removePostProcessor(pixelGlitchPostProcessor);
        }
        public void addHeatWave()
        {
            heatDistortionPostProcessor = entity.scene.addPostProcessor(new HeatDistortionPostProcessor(0));
        }
        public void removeHeatWave()
        {
            entity.scene.removePostProcessor(heatDistortionPostProcessor);
        }
        public void addScanlines()
        {
            scanLinesPostProcessor = entity.scene.addPostProcessor(new ScanlinesPostProcessor(0));
        }
        public void addNoise()
        {
            noisePostProcessor = entity.scene.addPostProcessor(new NoiseEffectPostProcessor(0));
        }
        void IUpdatable.update()
        {
            //moveScanlines();
            //movePixelGlitch();
            //moveNoise();
        }
        void moveScanlines()
        {
            var scanEffect = scanLinesPostProcessor.effect;
            //scanEffect.linesFactor += 1f;

            //scanEffect.attenuation += 0.1f;
        }
        public void increasePixelGlitch(float offsetIncrease)
        {
            pixelGlitchPostProcessor.horizontalOffset += offsetIncrease;
        }
        void moveNoise()
        {
            var noiseEffect = noisePostProcessor.effect;
            //noiseEffect.tr
        }
        public void resetPixelGlitch()
        {
            pixelGlitchPostProcessor.horizontalOffset = 10f;
        }
    }
}
