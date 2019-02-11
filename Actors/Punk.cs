using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EyesHaveIt.Enums;

namespace EyesHaveIt.Actors
{
    internal class Punk : Enemy, IUpdatable
    {
        private BoxCollider _punchCollider;
        private int _punchXOffset = -23;
        private int _punchYOffset = -20;
        private Vector2 _punchOffset = new Vector2(-23, -20);
        private int _damage = 1;
        private bool _isColliding = false;
        private bool _soundPlay = false;
        private List<ITriggerListener> _tempTriggerList = new List<ITriggerListener>();

        public Punk(String inEntityType, int inTotalLife, Nez.Tiled.TiledMap inMap, bool enemyHasGun, int ntargetPositionRange) : base(inEntityType, inTotalLife, inMap, enemyHasGun, ntargetPositionRange)
        {
            EntityType = inEntityType;
            TileMap = inMap;
            TotalLife = inTotalLife;
            CurrentLife = TotalLife;
            CollisionLayer = TileMap.getLayer<TiledTileLayer>("collisionLayer");
            WeightedGraph = new WeightedGridGraph(CollisionLayer);
            Start = new Point(1, 1);
            End = new Point(10, 10);
            WeightedSearchPath = WeightedGraph.search(Start, End);
            EnemyCanShootGun = enemyHasGun;
            TargetPositionRange = ntargetPositionRange;

        }
        public override void ColliderSetup()
        {
            
            Collider = entity.addComponent(new BoxCollider());
            Mover = entity.addComponent(new Mover());
            Flags.setFlagExclusive(ref Collider.collidesWithLayers, (int)Game1.PhysicsLayers.PlayerAttacksInactive);
            Flags.setFlagExclusive(ref Collider.physicsLayer, (int)Game1.PhysicsLayers.Enemy);
            HitController = entity.addComponent(new Utilities.HitController());
            _punchCollider = new BoxCollider();
            _punchCollider.setWidth(35f);
            _punchCollider.setHeight(5f);
            _punchCollider.setLocalOffset(_punchOffset);
            _punchCollider.isTrigger = true;
            Flags.setFlagExclusive(ref _punchCollider.collidesWithLayers, (int)Game1.PhysicsLayers.PlayerAttacksInactive);
            Flags.setFlagExclusive(ref _punchCollider.physicsLayer, (int)Game1.PhysicsLayers.EnemyAttacks);
            entity.addComponent(_punchCollider);
            //punchCollider.setEnabled(false);
        }
        public override void Punch()
        {
            
            Animation = EnemyAnimationState.Punch;
            Core.schedule(0.5f, false, newTimer => HandlePunchCollision());
            //Core.schedule(0.55f, false, newTimer => setPunchCollisionLayer(Game1.PhysicsLayers.PlayerAttacksInactive));
            //Core.schedule(0.5f, false, newTimer => soundPlay = false);
        }

        private void HandlePunchCollision()
        {
            PlayPunchSound();
            SetPunchCollisionLayer(Game1.PhysicsLayers.Player);
            var neighborColliders = Physics.boxcastBroadphaseExcludingSelf(_punchCollider);
            foreach (var neighbor in neighborColliders)
            {
                if (_punchCollider.overlaps(neighbor) && AnimationMaster.currentFrame > 0)
                {  
                    NotifyTriggerListeners(Collider, neighbor);
                }
            }
        }

        private void PlayPunchSound()
        {   
                if (AnimationMaster.currentFrame == 1 && !_soundPlay)
                {
                    _soundPlay = true;
                if (entity.scene!= null)
                {
                    entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().EnemyPunchEffect.Play();
                }
                    
                }
                if (AnimationMaster.currentFrame == 2 && _soundPlay)
                {
                    _soundPlay = false;
                }
        }

        private void SetPunchCollisionLayer(Game1.PhysicsLayers inLayer)
        {
            Flags.setFlagExclusive(ref _punchCollider.collidesWithLayers, (int)inLayer);
        }

        private void SetColliding(bool inBool)
        {
            //isColliding = inBool;
        }

        private void NotifyTriggerListeners(Collider self, Collider other)
        {
            // notify any listeners on the Entity of the Collider that we overlapped
            other.entity.getComponents(_tempTriggerList);
            for (var i = 0; i < _tempTriggerList.Count; i++)
                _tempTriggerList[i].onTriggerEnter(self, other);

            _tempTriggerList.Clear();

            // notify any listeners on this Entity
            entity.getComponents(_tempTriggerList);
            for (var i = 0; i < _tempTriggerList.Count; i++)
                _tempTriggerList[i].onTriggerEnter(other, self);

            _tempTriggerList.Clear();
        }

        private void PunkXFlip()
        {
            if (TargetPosition.X > entity.transform.position.X)
            {
                _punchCollider.setLocalOffset(new Vector2(-_punchOffset.X, _punchOffset.Y));
            }
            if (TargetPosition.X < entity.transform.position.X)
            {
                _punchCollider.setLocalOffset(new Vector2(_punchXOffset, _punchOffset.Y));
            }
        }
        void IUpdatable.update()
        {
            if (entity.scene != null && (Player.PlayerRef.PlayerState != PlayerState.Dead))
            {
                EnemyStateController();
                HandleAnimation();
                PunkXFlip();
                HandleRenderLayers();
                UpdateLocations();

                CheckIfDead();

            }

        }
    }
}
