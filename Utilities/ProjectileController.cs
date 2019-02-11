using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyesHaveIt.Utilities
{
    internal class ProjectileController : Component, IUpdatable
    {
        public Vector2 Velocity;
        private ProjectileMover _mover;
        private float _hitRange = 15;
        private Collider _otherCollider;
        private List<ITriggerListener> _tempTriggerList = new List<ITriggerListener>();
        private Collider _collider;



        public ProjectileController(Vector2 velocity)
        {
            this.Velocity = velocity;

        }


        public override void onAddedToEntity()
        {
            _mover = entity.getComponent<ProjectileMover>();
            _collider = entity.getComponent<Collider>();
        }

        private void CheckBoundaries()
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

            if (_mover.move(Velocity * Time.deltaTime))
            {
                _otherCollider = _mover.entity.getComponent<Collider>();
                entity.destroy();
            }
            CheckBoundaries();


        }

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
