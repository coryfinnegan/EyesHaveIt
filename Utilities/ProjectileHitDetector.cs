using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using Nez.Sprites;
using Microsoft.Xna.Framework;

namespace EyesHaveIt.Utilities
{
    public class ProjectileHitDetector : Component, ITriggerListener
    {
        public int hitsUntilDead = 10;
        public int _hitCounter;
        Sprite _sprite;
        bool beenHit= false;
        float hitRange = 15;


        public override void onAddedToEntity()
        {
            _sprite = entity.getComponent<Sprite>();
        }


        void ITriggerListener.onTriggerEnter(Collider other, Collider self)
        {
            
            if (hitsUntilDead > 0)
            {
                if (!beenHit && checkYValue(other))
                {
                    beenHit = true;
                    Debug.log("Player Hit by: " + other.entity.name);
                    hitsUntilDead--;
                    
                    _sprite.color = Color.Red;
                    Core.schedule(0.1f, timer => _sprite.color = Color.White);
                    Core.schedule(1f, timer2 => setBeenHit(false));
                }
            }
            else
            {
                entity.getComponent<Player>().setState(Player.PlayerState.Dead);
            }
         
            //if (other.entity.)

        }
        void setBeenHit(bool inBool)
        {
            beenHit = inBool;
        }

        void ITriggerListener.onTriggerExit(Collider other, Collider self)
        { }
        bool checkYValue(Collider inCollider)
        {
            //var hitRange = 10;
            var enemyYPos = inCollider.entity.transform.position.Y;
            var playerYPos = transform.position.Y;
            if ((enemyYPos - playerYPos) >= -hitRange && (enemyYPos - playerYPos) <= hitRange)
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
