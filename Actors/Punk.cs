using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyesHaveIt.Actors
{
    class Punk : Enemy, IUpdatable
    {
        BoxCollider punchCollider;
        int punchXOffset = -23;
        int punchYOffset = -20;
        Vector2 punchOffset = new Vector2(-23, -20);
        int damage = 1;
        bool isColliding = false;
        bool soundPlay = false;
        List<ITriggerListener> _tempTriggerList = new List<ITriggerListener>();

        public Punk(String inEntityType, int inTotalLife, Nez.Tiled.TiledMap inMap, bool enemyHasGun, int INtargetPositionRange) : base(inEntityType, inTotalLife, inMap, enemyHasGun, INtargetPositionRange)
        {
            entityType = inEntityType;
            _tileMap = inMap;
            totalLife = inTotalLife;
            currentLife = totalLife;
            _collisionLayer = _tileMap.getLayer<TiledTileLayer>("collisionLayer");
            _weightedGraph = new WeightedGridGraph(_collisionLayer);
            _start = new Point(1, 1);
            _end = new Point(10, 10);
            _weightedSearchPath = _weightedGraph.search(_start, _end);
            enemyCanShootGun = enemyHasGun;
            targetPositionRange = INtargetPositionRange;

        }
        public override void colliderSetup()
        {
            
            _collider = entity.addComponent(new BoxCollider());
            _mover = entity.addComponent(new Mover());
            Flags.setFlagExclusive(ref _collider.collidesWithLayers, (int)Game1.PhysicsLayers.PlayerAttacksInactive);
            Flags.setFlagExclusive(ref _collider.physicsLayer, (int)Game1.PhysicsLayers.Enemy);
            hitController = entity.addComponent(new Utilities.HitController());
            punchCollider = new BoxCollider();
            punchCollider.setWidth(35f);
            punchCollider.setHeight(5f);
            punchCollider.setLocalOffset(punchOffset);
            punchCollider.isTrigger = true;
            Flags.setFlagExclusive(ref punchCollider.collidesWithLayers, (int)Game1.PhysicsLayers.PlayerAttacksInactive);
            Flags.setFlagExclusive(ref punchCollider.physicsLayer, (int)Game1.PhysicsLayers.EnemyAttacks);
            entity.addComponent(punchCollider);
            //punchCollider.setEnabled(false);
        }
        public override void punch()
        {
            
            animation = Animations.Punch;
            Core.schedule(0.5f, false, newTimer => handlePunchCollision());
            //Core.schedule(0.55f, false, newTimer => setPunchCollisionLayer(Game1.PhysicsLayers.PlayerAttacksInactive));
            //Core.schedule(0.5f, false, newTimer => soundPlay = false);
        }
        void handlePunchCollision()
        {
            playPunchSound();
            setPunchCollisionLayer(Game1.PhysicsLayers.Player);
            var neighborColliders = Physics.boxcastBroadphaseExcludingSelf(punchCollider);
            foreach (var neighbor in neighborColliders)
            {
                if (punchCollider.overlaps(neighbor) && _animationMaster.currentFrame > 0)
                {  
                    notifyTriggerListeners(_collider, neighbor);
                }
            }
        }
        void playPunchSound()
        {   
                if (_animationMaster.currentFrame == 1 && !soundPlay)
                {
                    soundPlay = true;
                if (entity.scene!= null)
                {
                    entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().enemyPunchEffect.Play();
                }
                    
                }
                if (_animationMaster.currentFrame == 2 && soundPlay)
                {
                    soundPlay = false;
                }
        }
        void setPunchCollisionLayer(Game1.PhysicsLayers inLayer)
        {
            Flags.setFlagExclusive(ref punchCollider.collidesWithLayers, (int)inLayer);
        }
        void setColliding(bool inBool)
        {
            //isColliding = inBool;
        }
        void notifyTriggerListeners(Collider self, Collider other)
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
        void punkXFlip()
        {
            if (targetPosition.X > entity.transform.position.X)
            {
                punchCollider.setLocalOffset(new Vector2(-punchOffset.X, punchOffset.Y));
            }
            if (targetPosition.X < entity.transform.position.X)
            {
                punchCollider.setLocalOffset(new Vector2(punchXOffset, punchOffset.Y));
            }
        }
        void IUpdatable.update()
        {
            if (entity.scene != null && (Player.playerRef.playerState != Player.PlayerState.Dead))
            {
                entityStateController();
                handleAnimation();
                punkXFlip();
                handleRenderLayers();
                updateLocations();

                checkIfDead();

            }

        }
    }
}
