using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace EyesHaveIt.Utilities
{
    class AudioController : Component
    {
        public SoundEffect playerPunchEffect { private set; get; }
        public SoundEffect enemyPunchEffect { private set; get; }
        public SoundEffect enemyShootEffect { private set; get; }
        public SoundEffect enemyDieEffect { private set; get; }
        public Song backgroundMusic { private set; get; }
        public SoundEffect enemyHitEffect { private set; get; }
        public SoundEffectInstance playerPunch { private set; get; }
        public SoundEffectInstance enemyPunch { private set; get; }
        public SoundEffectInstance enemyShoot { private set; get; }
        public SoundEffectInstance enemyDie { private set; get; }
        public SoundEffectInstance enemyHit { private set; get; }





        public AudioController()
        {

        }
        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            playerPunchEffect = entity.scene.content.Load<SoundEffect>("Sound/playerPunch"); //Yes
            enemyPunchEffect = entity.scene.content.Load<SoundEffect>("Sound/enemyPunch"); //yes
            enemyShootEffect = entity.scene.content.Load<SoundEffect>("Sound/enemyShoot"); //Yes
            enemyDieEffect = entity.scene.content.Load<SoundEffect>("Sound/enemyDie"); //Yes

            backgroundMusic = entity.scene.content.Load<Song>("Sound/backgroundMusic");//yes
            playBackgroundMusic();


            enemyHitEffect = entity.scene.content.Load<SoundEffect>("Sound/enemyHit"); //Yes

            playerPunch = playerPunchEffect.CreateInstance();
            enemyPunch = enemyPunchEffect.CreateInstance();
            enemyShoot = enemyShootEffect.CreateInstance();
            enemyDie = enemyDieEffect.CreateInstance();
            enemyHit = enemyHitEffect.CreateInstance();
            


        }
        public void playBackgroundMusic()
        {
            MediaPlayer.Play(backgroundMusic);
            MediaPlayer.IsRepeating = true;
        }

    }
}
