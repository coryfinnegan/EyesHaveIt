using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyesHaveIt.Utilities
{
    class ProjectileController : Component, IUpdatable
    {
        public Vector2 velocity;

        ProjectileMover _mover;
        float hitRange = 15;
        Collider otherCollider;
        List<ITriggerListener> _tempTriggerList = new List<ITriggerListener>();
        Collider _collider;



        public ProjectileController(Vector2 velocity)
        {
            this.velocity = velocity;

        }


        public override void onAddedToEntity()
        {
            _mover = entity.getComponent<ProjectileMover>();
            _collider = entity.getComponent<Collider>();
        }
        void checkBoundaries()
        {
            if (transform.position.X > entity.scene.camera.bounds.right)
            {
                entity.destroy();
            }
            if (transform.position.X < entity.scene.camera.bounds.left)
            {
                entity.destroy();
            }
        }

        void IUpdatable.update()
        {

            if (_mover.move(velocity * Time.deltaTime))
            {
                otherCollider = _mover.getOtherCollider();
                entity.destroy();
                //if (_mover.checkYValue(otherCollider))
                //{
                 //   entity.destroy();
                //}
            }
            checkBoundaries();


        }
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
