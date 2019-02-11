using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EyesHaveIt.Enums;
using Nez;
using Nez.Sprites;
using Microsoft.Xna.Framework;

namespace EyesHaveIt.Utilities
{
    public class ProjectileHitDetector : Component, ITriggerListener
    {
        public int HitsUntilDead = 10;
        public int HitCounter;
        private Sprite _sprite;
        private bool _beenHit= false;
        private float _hitRange = 15;


        public override void onAddedToEntity()
        {
            _sprite = entity.getComponent<Sprite>();
        }


        void ITriggerListener.onTriggerEnter(Collider other, Collider self)
        {
            
            if (HitsUntilDead > 0)
            {
                if (!_beenHit && CheckYValue(other))
                {
                    _beenHit = true;
                    Debug.log("Player Hit by: " + other.entity.name);
                    HitsUntilDead--;
                    
                    _sprite.color = Color.Red;
                    Core.schedule(0.1f, timer => _sprite.color = Color.White);
                    Core.schedule(1f, timer2 => SetBeenHit(false));
                }
            }
            else
            {
                entity.getComponent<Player>().SetState(PlayerState.Dead);
            }
         
            //if (other.entity.)

        }

        private void SetBeenHit(bool inBool)
        {
            _beenHit = inBool;
        }

        void ITriggerListener.onTriggerExit(Collider other, Collider self)
        { }

        private bool CheckYValue(Collider inCollider)
        {
            //var hitRange = 10;
            var enemyYPos = inCollider.entity.transform.position.Y;
            var playerYPos = transform.position.Y;
            if ((enemyYPos - playerYPos) >= -_hitRange && (enemyYPos - playerYPos) <= _hitRange)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
