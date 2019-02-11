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
    internal class AudioController : Component
    {
        public SoundEffect PlayerPunchEffect { private set; get; }
        public SoundEffect EnemyPunchEffect { private set; get; }
        public SoundEffect EnemyShootEffect { private set; get; }
        public SoundEffect EnemyDieEffect { private set; get; }
        public Song BackgroundMusic { private set; get; }
        public SoundEffect EnemyHitEffect { private set; get; }
        public SoundEffectInstance PlayerPunch { private set; get; }
        public SoundEffectInstance EnemyPunch { private set; get; }
        public SoundEffectInstance EnemyShoot { private set; get; }
        public SoundEffectInstance EnemyDie { private set; get; }
        public SoundEffectInstance EnemyHit { private set; get; }





        public AudioController()
        {

        }
        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            PlayerPunchEffect = entity.scene.content.Load<SoundEffect>("Sound/playerPunch"); //Yes
            EnemyPunchEffect = entity.scene.content.Load<SoundEffect>("Sound/enemyPunch"); //yes
            EnemyShootEffect = entity.scene.content.Load<SoundEffect>("Sound/enemyShoot"); //Yes
            EnemyDieEffect = entity.scene.content.Load<SoundEffect>("Sound/enemyDie"); //Yes

            BackgroundMusic = entity.scene.content.Load<Song>("Sound/backgroundMusic");//yes
            PlayBackgroundMusic();


            EnemyHitEffect = entity.scene.content.Load<SoundEffect>("Sound/enemyHit"); //Yes

            PlayerPunch = PlayerPunchEffect.CreateInstance();
            EnemyPunch = EnemyPunchEffect.CreateInstance();
            EnemyShoot = EnemyShootEffect.CreateInstance();
            EnemyDie = EnemyDieEffect.CreateInstance();
            EnemyHit = EnemyHitEffect.CreateInstance();
            


        }
        public void PlayBackgroundMusic()
        {
            MediaPlayer.Play(BackgroundMusic);
            MediaPlayer.IsRepeating = true;
        }

    }
}
