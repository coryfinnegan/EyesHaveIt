using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;

namespace EyesHaveIt.Utilities
{
    internal class PostEffectController : Component, IUpdatable
    {
        private Scene _myScene;
        public GrayscalePostProcessor GreyScalePostProcessor { get; set; }
        public PixelGlitchPostProcessor PixelGlitchPostProcessor { get; set; }
        public HeatDistortionPostProcessor HeatDistortionPostProcessor { get; set; }

        private ScanlinesPostProcessor _scanLinesPostProcessor;
        private NoiseEffectPostProcessor _noisePostProcessor;

        public PostEffectController()
        {
        }
        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
        }
        public void AddGreyScale()
        {
            GreyScalePostProcessor = entity.scene.addPostProcessor(new GrayscalePostProcessor(0));
        }
        public void RemoveGreyScale()
        {
            entity.scene.removePostProcessor(GreyScalePostProcessor);
        }
        public void AddPixelGlitch()
        {
            PixelGlitchPostProcessor = entity.scene.addPostProcessor(new PixelGlitchPostProcessor(0));
        }
        public void RemovePixelGlitch()
        {
            entity.scene.removePostProcessor(PixelGlitchPostProcessor);
        }
        public void AddHeatWave()
        {
            HeatDistortionPostProcessor = entity.scene.addPostProcessor(new HeatDistortionPostProcessor(0));
        }
        public void RemoveHeatWave()
        {
            entity.scene.removePostProcessor(HeatDistortionPostProcessor);
        }
        public void AddScanlines()
        {
            _scanLinesPostProcessor = entity.scene.addPostProcessor(new ScanlinesPostProcessor(0));
        }
        public void AddNoise()
        {
            _noisePostProcessor = entity.scene.addPostProcessor(new NoiseEffectPostProcessor(0));
        }
        void IUpdatable.update()
        {
            //moveScanlines();
            //movePixelGlitch();
            //moveNoise();
        }

        private void MoveScanlines()
        {
            var scanEffect = _scanLinesPostProcessor.effect;
            //scanEffect.linesFactor += 1f;

            //scanEffect.attenuation += 0.1f;
        }
        public void IncreasePixelGlitch(float offsetIncrease)
        {
            PixelGlitchPostProcessor.horizontalOffset += offsetIncrease;
        }

        private void MoveNoise()
        {
            var noiseEffect = _noisePostProcessor.effect;
            //noiseEffect.tr
        }
        public void ResetPixelGlitch()
        {
            PixelGlitchPostProcessor.horizontalOffset = 10f;
        }
    }
}
