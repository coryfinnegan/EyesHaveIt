using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.AI.Pathfinding;
using Microsoft.Xna.Framework.Graphics;

namespace EyesHaveIt
{
    public class Enemy : Component, IUpdatable//, ITriggerListener
    {
        protected enum Animations
        {
            Walk,
            Shoot,
            Die,
            Idle,
            Fall,
            GetUp,
            Hit,
            Punch
        }
        public enum EntityState
        {
            Following,
            Attacking,
            Hit,
            Falling,
            OnGround,
            GettingUp,
            RunAway,
            Dying
        }
        protected Game1 gameRef;
        protected Player playerRef;
        [Inspectable]
        protected EntityState entityState;
        protected String entityType;
        protected bool enemyCanShootGun;

        protected Mover _mover;
        //TiledMapMover _mover;
        protected BoxCollider _collider;


        protected Sprite<Animations> _animationMaster;
        protected WeightedGridGraph _weightedGraph;
        protected List<Point> _weightedSearchPath;
        protected TiledMap _tileMap;
        protected TiledTileLayer _collisionLayer;
        protected Point _start, _end;
        protected TiledObject playerSpawn;
        protected Vector2 spawnLocation;
        protected float _moveSpeed = 75f;
        protected Animations animation;
        protected Vector2 movement;
        protected Vector2 targetPosition;
        protected int targetPositionRange;
        [Inspectable]
        public float totalLife { get; set; }//Must always be even
        [Inspectable]
        public float currentLife { get; set; }
        public Utilities.HitController hitController;
        protected float jumpHeight = 5f;
        protected float gravity = 30f;
        protected bool onGround = false;
        protected bool isFalling = false;
        protected bool hasFallen = false;
        protected float currentYValue;
        protected float fallSpeed = 20f;
        [Inspectable]
        protected float fallSpeedX = 20f;
        protected bool alive = true;

        //Projectile
        [Inspectable]
        bool canShoot { get; set; }
        bool canBeHit;
        Texture2D bulletTexture;
        Vector2 outVel = new Vector2(-175, 0);
        Vector2 shootOffset = new Vector2(45, 30);
        ITimer shootTask;






        public Enemy(String inEntityType, int inTotalLife, TiledMap inMap, bool enemyHasGun, int INtargetPositionRange)
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
            playerRef = Player.playerRef;
            //.setTag((int)Scenes.LevelOne.Tags.enemy);

        }
        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            animationSetup();
            colliderSetup();
            pathfindingSetup();

            if (enemyCanShootGun)
            {
                bulletTexture = entity.scene.content.Load<Texture2D>("characters/" + entityType + "/" + entityType + "Bullet");
                canShoot = true;

            }
        }
        void pathfindingSetup()
        {
            spawnLocation = transform.position;
            playerSpawn = _tileMap.getObjectGroup("objects").objectWithName("spawn");
            _start = new Point((int)spawnLocation.X, (int)spawnLocation.Y);
            _end = new Point(playerSpawn.x, playerSpawn.y);
            _weightedSearchPath = _weightedGraph.search(_start, _end);
            setState(EntityState.Following);
        }
        void animationSetup()
        {
            var atlas = entity.scene.content.Load<TextureAtlas>("characters/" + entityType + "/" + entityType + "Atlas");
            var walk = atlas.getSpriteAnimation(entityType + "Walk");
            walk.setFps(5f);
            walk.setPingPong(true);
            _animationMaster = entity.addComponent(new Sprite<Animations>(Animations.Walk, walk));
            if (enemyCanShootGun)
            {
                var shoot = atlas.getSpriteAnimation(entityType + "Shoot");
                shoot.setFps(3f);
                _animationMaster.addAnimation(Animations.Shoot, shoot);
            }
            if (!enemyCanShootGun)
            {
                var punch = atlas.getSpriteAnimation(entityType + "Punch");
                punch.setFps(3f);
                _animationMaster.addAnimation(Animations.Punch, punch);
            }
            var die = atlas.getSpriteAnimation(entityType + "Die");
            die.setFps(100f);

            var idle = atlas.getSpriteAnimation(entityType + "Idle");
            var fall = atlas.getSpriteAnimation(entityType + "Fall");
            var getUp = atlas.getSpriteAnimation(entityType + "GetUp");
            var hit = atlas.getSpriteAnimation(entityType + "Hit");
            getUp.loop = false;
            getUp.setFps(1f);
            _animationMaster.addAnimation(Animations.Die, die);
            _animationMaster.addAnimation(Animations.Idle, idle);
            _animationMaster.addAnimation(Animations.Fall, fall);
            _animationMaster.addAnimation(Animations.GetUp, getUp);
            _animationMaster.addAnimation(Animations.Hit, hit);

        }
        public virtual void colliderSetup()
        {
            _collider = entity.addComponent(new BoxCollider());
            _collider.setWidth(28f);
            _collider.setHeight(79f);
            //_collider.setLocalOffset(new Vector2(-14, -14));
            _collider.setLocalOffset(new Vector2(14, 0));
            _mover = entity.addComponent(new Mover());
            Flags.setFlagExclusive(ref _collider.collidesWithLayers, (int)Game1.PhysicsLayers.PlayerAttacksInactive);
            Flags.setFlagExclusive(ref _collider.physicsLayer, (int)Game1.PhysicsLayers.Enemy);
            hitController = entity.addComponent(new Utilities.HitController());
        }
        void IUpdatable.update()
        {
            if (entity.scene != null && (Player.playerRef.playerState != Player.PlayerState.Dead))
            {
                entityStateController();
                handleAnimation();
                handleRenderLayers();
                updateLocations();
                checkIfDead();
            }
     
        }
        protected void entityStateController()
        {
            switch (entityState)
            {
                case EntityState.Following:
                    followPlayer();
                    break;
                case EntityState.Falling:
                    fallOnGround();
                    break;
                case EntityState.OnGround:
                    handleOnGround();
                    break;
                case EntityState.Dying:
                    die();
                    break;
                case EntityState.GettingUp:
                    getup();
                    break;
                case EntityState.Hit:
                    getHit();
                    break;
                case EntityState.Attacking:
                    attack();

                    break;
            }
        }
        public void setState(EntityState inState)
        {
            if (entity.scene != null)
            {
                Debug.log("Setting state to: " + inState);
                entityState = inState;
            }
           
        }
        protected void handleRenderLayers()
        {
            entity.getComponent<RenderableComponent>().renderLayer = -(int)transform.position.Y;
        }
        protected void handleAnimation()
        {
            detectXFlip();
            playAnimation();
        }
        void playAnimation()
        {
            if (!_animationMaster.isAnimationPlaying(animation))
            {
                _animationMaster.play(animation);
            }
        }
        void slowDownBasedOnPlayerAttack()
        {
            if (playerRef.getPunching())
            {
                _moveSpeed = 25f;
            }
            else if (!playerRef.getPunching())
            {
                _moveSpeed = 75f;
            }
        }
        void walkAnimation()
        {
            animation = Animations.Walk;
            if (targetPosition.X > entity.transform.position.X)
            {
                _collider.setLocalOffset(new Vector2(-14, 0));
                //_animationMaster.flipX = true;
            }
            if (targetPosition.X < entity.transform.position.X)
            {
                _collider.setLocalOffset(new Vector2(14, 0));
            }
            if (_weightedSearchPath != null && _weightedSearchPath.Count() <= targetPositionRange)
            {
                animation = Animations.Idle;
            }
        }
        void detectXFlip()
        {
            if (targetPosition.X > entity.transform.position.X)
            {
                _animationMaster.flipX = true;

                if (fallSpeedX > 0)
                {
                    outVel = new Vector2(Math.Abs(outVel.X), 0);
                    fallSpeedX = -fallSpeedX;
                    shootOffset.X = -shootOffset.X;
                }

            }

            if (targetPosition.X < entity.transform.position.X)
            {
                if (outVel.X > 0)
                {
                    outVel = -outVel;
                }
                _animationMaster.flipX = false;
                fallSpeedX = Math.Abs(fallSpeedX);
                shootOffset.X = Math.Abs(shootOffset.X);
            }

        }
        void fallOnGround()
        {

            //falling
            if (!isFalling && !onGround)
            {
                animation = Animations.Fall;
                
                currentYValue = transform.position.Y;
                Debug.log("Current Y Value: " + currentYValue);
                Debug.log("Current Transform.position.Y: " + transform.position.Y);
                movement.Y = -Mathf.sqrt(jumpHeight * gravity);
                isFalling = true;
            }
            if (isFalling)
            {
                //animation = Animations.Fall;
                movement.X += fallSpeedX * Time.deltaTime;
                movement.Y += gravity * Time.deltaTime;
                //movement.Normalize();
                CollisionResult res;
                _mover.move(movement * fallSpeed * Time.deltaTime, out res);
                if (transform.position.Y >= currentYValue)
                {
                    transform.setPosition(new Vector2(transform.position.X, currentYValue));
                    Debug.log("Current Y Value after ground hit: " + currentYValue);
                    Debug.log("Current Transform.position.Y after ground hit: " + transform.position.Y);
                    isFalling = false;
                    onGround = true;
                    setState(EntityState.OnGround);
                }
            }
        }
        void handleOnGround()
        {
            movement = Vector2.Zero;
            animation = Animations.Die;
            if (currentLife > 0)
            {
                setState(EntityState.GettingUp);

            }
            if (currentLife <= 0)
            {
                setState(EntityState.Dying);
            }
        }
        void getup()
        {
            animation = Animations.GetUp;
            onGround = false;
            isFalling = false;
            ITimer task = Core.schedule(2f, false, timer => this.setState(EntityState.Following));
        }
        void die()
        {
            if (alive)
            {
                alive = false;
                animation = Animations.Die;
                entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().enemyDieEffect.Play();
                var task = Core.schedule(3f, false, timer => entity.detachFromScene());
            }
        }
        protected void updateLocations()
        {
            var gameScene = entity.scene;
            var player = gameScene.findObjectOfType<Player>();
            _start = _tileMap.worldToTilePosition(entity.transform.position);

            _end = _tileMap.worldToTilePosition(player.transform.position);
            _weightedSearchPath = _weightedGraph.search(_start, _end);
        }
        bool shouldFollowPlayer()
        {
            if (_weightedSearchPath != null && _weightedSearchPath.Count() > targetPositionRange)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        void followPlayer()
        {
            //can be hit set true
            if (!canShoot)
                canShoot = true;
            if (!canBeHit)
                canBeHit = true;
            walkAnimation();

            if (shouldFollowPlayer())
            {
                slowDownBasedOnPlayerAttack();
                targetPosition = _tileMap.tileToWorldPosition(_weightedSearchPath.Last());
                float x = targetPosition.X - entity.transform.position.X;
                float y = targetPosition.Y - entity.transform.position.Y;
                x = (float)(x / Math.Sqrt(x * x + y * y));
                y = (float)(y / Math.Sqrt(x * x + y * y));
                Vector2 moveDirection = new Vector2(x, y);
                movement = moveDirection * _moveSpeed * Time.deltaTime;
                CollisionResult res;
                _mover.move(movement, out res);
            }
            else if (!shouldFollowPlayer())
            {
                setState(EntityState.Attacking);
            }

        }

        void getHit()
        {
            if (canBeHit)
            {
                canBeHit = false;
                animation = Animations.Hit;
                if (_weightedSearchPath != null && _weightedSearchPath.Count() > targetPositionRange)
                {
                    ITimer newTask = Core.schedule(0.3f, false, newTimer => this.setState(EntityState.Following));
                }
                else if (_weightedSearchPath != null && _weightedSearchPath.Count() <= targetPositionRange)
                {
                    ITimer newTask = Core.schedule(0.3f, false, newTimer => this.setState(EntityState.Attacking));
                }
            }


        }
        void attack()
        {
            if (!shouldFollowPlayer())
            {
                if (enemyCanShootGun)
                {
                    shoot();
                }
                if (!enemyCanShootGun)
                {
                    punch();
                }
            }
            if (shouldFollowPlayer())
            {
                setState(EntityState.Following);
            }

        }
        public virtual void punch()
        {

            animation = Animations.Punch;
        }

        public void handleHits(float inDamage)
        {
            if (entityState == EntityState.Following ||
                entityState == EntityState.Attacking ||
                entityState == EntityState.RunAway)
            {
                currentLife -= inDamage;
                if (currentLife == 0)
                {
                    setState(EntityState.Falling);
                }
                if (currentLife != 0 && currentLife > totalLife / 2)
                {
                    entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().enemyHitEffect.Play();
                    setState(EntityState.Hit);
                }
                if (currentLife != 0 && currentLife <= totalLife / 2 && !hasFallen)
                {
                    setState(EntityState.Falling);
                }
                if (currentLife != 0 && currentLife <= totalLife && hasFallen)
                {
                    entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().enemyHitEffect.Play();
                    setState(EntityState.Hit);
                }
            }
        }
        public void makeBullet(Vector2 position, Vector2 velocity)
        {
            if (entity.scene!= null)
            {
                if (entityState == EntityState.Attacking)
                {
                    var projectile = entity.scene.createEntity("projectile");
                    projectile.position = position;
                    var collider = projectile.addComponent<BoxCollider>();
                    projectile.addComponent(new ProjectileMover());
                    projectile.addComponent(new Utilities.ProjectileController(velocity));
                    Flags.setFlagExclusive(ref collider.collidesWithLayers, (int)Game1.PhysicsLayers.Player);
                    Flags.setFlagExclusive(ref collider.physicsLayer, (int)Game1.PhysicsLayers.EnemyAttacks);
                    projectile.scale = new Vector2(1.5f);
                    var sprite = projectile.addComponent(new Sprite(bulletTexture));
                    sprite.renderLayer = entity.getComponent<RenderableComponent>().renderLayer;
                    entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().enemyShootEffect.Play();

                    Core.schedule(1f, false, newTimer => setShootTrue());
                }
            }
        }
        void setShootTrue()
        {
            if (entity.scene!= null)
            {
                canShoot = true;
            }
        }
        void shoot()
        {
            if (entity.scene!= null)
            {
                animation = Animations.Shoot;
                if (!canBeHit)
                    canBeHit = true;
                if (canShoot)
                {
                    canShoot = false;
                    animation = Animations.Shoot;
                    shootTask = Core.schedule(1f, false, newTimer => makeBullet(transform.position - shootOffset, outVel));
                }
            }

        }
        void checkIfStopShooting()
        {
            if (entityState != EntityState.Attacking && shootTask != null)
            {
                shootTask.stop();
            }
        }
        protected void checkIfDead()
        {
            if (currentLife == 0)
            {
                if (entityState != EntityState.Falling && entityState != EntityState.OnGround && entityState != EntityState.Dying)
                    setState(EntityState.Falling);
                //if (animation != Animations.)
            }
           
        }
    }//End class
}//end namespace
