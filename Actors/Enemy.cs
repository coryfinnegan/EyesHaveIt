using System;
using System.Collections.Generic;
using System.Linq;
using EyesHaveIt.Enums;
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
        protected Game1 GameRef;
        protected Player PlayerRef;
        [Inspectable]
        protected EnemyState EnemyState;
        protected String EntityType;
        protected bool EnemyCanShootGun;

        protected Mover Mover;
        //TiledMapMover _mover;
        protected BoxCollider Collider;


        protected Sprite<EnemyAnimationState> AnimationMaster;
        protected WeightedGridGraph WeightedGraph;
        protected List<Point> WeightedSearchPath;
        protected TiledMap TileMap;
        protected TiledTileLayer CollisionLayer;
        protected Point Start, End;
        protected TiledObject PlayerSpawn;
        protected Vector2 SpawnLocation;
        protected float MoveSpeed = 75f;
        protected EnemyAnimationState Animation;
        protected Vector2 Movement;
        protected Vector2 TargetPosition;
        protected int TargetPositionRange;
        [Inspectable]
        public float TotalLife { get; set; }//Must always be even
        [Inspectable]
        public float CurrentLife { get; set; }
        public Utilities.HitController HitController;
        protected float JumpHeight = 5f;
        protected float Gravity = 30f;
        protected bool OnGround = false;
        protected bool IsFalling = false;
        protected bool HasFallen = false;
        protected float CurrentYValue;
        protected float FallSpeed = 20f;
        [Inspectable]
        protected float FallSpeedX = 20f;
        protected bool Alive = true;

        //Projectile
        [Inspectable]
        private bool CanShoot { get; set; }

        private bool _canBeHit;
        private Texture2D _bulletTexture;
        private Vector2 _outVel = new Vector2(-175, 0);
        private Vector2 _shootOffset = new Vector2(45, 30);
        private ITimer _shootTask;

        public Enemy(String inEntityType, int inTotalLife, TiledMap inMap, bool enemyHasGun, int ntargetPositionRange)
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
            PlayerRef = Player.PlayerRef;

        }
        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            entity.setTag((int) Tags.Enemy);
            EnemyAnimationStateetup();
            ColliderSetup();
            PathfindingSetup();

            if (EnemyCanShootGun)
            {
                _bulletTexture = entity.scene.content.Load<Texture2D>("characters/" + EntityType + "/" + EntityType + "Bullet");
                CanShoot = true;

            }
        }

        private void PathfindingSetup()
        {
            SpawnLocation = transform.position;
            PlayerSpawn = TileMap.getObjectGroup("objects").objectWithName("spawn");
            Start = new Point((int)SpawnLocation.X, (int)SpawnLocation.Y);
            End = new Point(PlayerSpawn.x, PlayerSpawn.y);
            WeightedSearchPath = WeightedGraph.search(Start, End);
            SetState(EnemyState.Following);
        }

        private void EnemyAnimationStateetup()
        {
            var atlas = entity.scene.content.Load<TextureAtlas>("characters/" + EntityType + "/" + EntityType + "Atlas");
            var walk = atlas.getSpriteAnimation(EntityType + "Walk");
            walk.setFps(5f);
            walk.setPingPong(true);
            AnimationMaster = entity.addComponent(new Sprite<EnemyAnimationState>(EnemyAnimationState.Walk, walk));
            if (EnemyCanShootGun)
            {
                var shoot = atlas.getSpriteAnimation(EntityType + "Shoot");
                shoot.setFps(3f);
                AnimationMaster.addAnimation(EnemyAnimationState.Shoot, shoot);
            }
            if (!EnemyCanShootGun)
            {
                var punch = atlas.getSpriteAnimation(EntityType + "Punch");
                punch.setFps(3f);
                AnimationMaster.addAnimation(EnemyAnimationState.Punch, punch);
            }
            var die = atlas.getSpriteAnimation(EntityType + "Die");
            die.setFps(100f);

            var idle = atlas.getSpriteAnimation(EntityType + "Idle");
            var fall = atlas.getSpriteAnimation(EntityType + "Fall");
            var getUp = atlas.getSpriteAnimation(EntityType + "GetUp");
            var hit = atlas.getSpriteAnimation(EntityType + "Hit");
            getUp.loop = false;
            getUp.setFps(1f);
            AnimationMaster.addAnimation(EnemyAnimationState.Die, die);
            AnimationMaster.addAnimation(EnemyAnimationState.Idle, idle);
            AnimationMaster.addAnimation(EnemyAnimationState.Fall, fall);
            AnimationMaster.addAnimation(EnemyAnimationState.GetUp, getUp);
            AnimationMaster.addAnimation(EnemyAnimationState.Hit, hit);

        }

        public virtual void ColliderSetup()
        {
            Collider = entity.addComponent(new BoxCollider());
            Collider.setWidth(28f);
            Collider.setHeight(79f);
            //_collider.setLocalOffset(new Vector2(-14, -14));
            Collider.setLocalOffset(new Vector2(14, 0));
            Mover = entity.addComponent(new Mover());
            Flags.setFlagExclusive(ref Collider.collidesWithLayers, (int)Game1.PhysicsLayers.PlayerAttacksInactive);
            Flags.setFlagExclusive(ref Collider.physicsLayer, (int)Game1.PhysicsLayers.Enemy);
            HitController = entity.addComponent(new Utilities.HitController());
        }

        void IUpdatable.update()
        {
            if (entity.scene != null && (Player.PlayerRef.PlayerState != Enums.PlayerState.Dead))
            {
                EnemyStateController();
                HandleAnimation();
                HandleRenderLayers();
                UpdateLocations();
                CheckIfDead();
            }
     
        }

        protected void EnemyStateController()
        {
            switch (EnemyState)
            {
                case EnemyState.Following:
                    FollowPlayer();
                    break;
                case EnemyState.Falling:
                    FallOnGround();
                    break;
                case EnemyState.OnGround:
                    HandleOnGround();
                    break;
                case EnemyState.Dying:
                    Die();
                    break;
                case EnemyState.GettingUp:
                    Getup();
                    break;
                case EnemyState.Hit:
                    GetHit();
                    break;
                case EnemyState.Attacking:
                    Attack();

                    break;
            }
        }

        public void SetState(EnemyState inState)
        {
            if (entity.scene != null)
            {
                Debug.log("Setting state to: " + inState);
                EnemyState = inState;
            }
           
        }

        protected void HandleRenderLayers()
        {
            entity.getComponent<RenderableComponent>().renderLayer = -(int)transform.position.Y;
        }

        protected void HandleAnimation()
        {
            DetectXFlip();
            PlayAnimation();
        }

        private void PlayAnimation()
        {
            if (!AnimationMaster.isAnimationPlaying(Animation))
            {
                AnimationMaster.play(Animation);
            }
        }

        private void SlowDownBasedOnPlayerAttack()
        {
            if (PlayerRef.IsPunching())
            {
                MoveSpeed = 25f;
            }
            else if (!PlayerRef.IsPunching())
            {
                MoveSpeed = 75f;
            }
        }

        private void WalkAnimation()
        {
            Animation = EnemyAnimationState.Walk;
            if (TargetPosition.X > entity.transform.position.X)
            {
                Collider.setLocalOffset(new Vector2(-14, 0));
                //_animationMaster.flipX = true;
            }
            if (TargetPosition.X < entity.transform.position.X)
            {
                Collider.setLocalOffset(new Vector2(14, 0));
            }
            if (WeightedSearchPath != null && WeightedSearchPath.Count() <= TargetPositionRange)
            {
                Animation = EnemyAnimationState.Idle;
            }
        }

        private void DetectXFlip()
        {
            if (TargetPosition.X > entity.transform.position.X)
            {
                AnimationMaster.flipX = true;

                if (FallSpeedX > 0)
                {
                    _outVel = new Vector2(Math.Abs(_outVel.X), 0);
                    FallSpeedX = -FallSpeedX;
                    _shootOffset.X = -_shootOffset.X;
                }

            }

            if (TargetPosition.X < entity.transform.position.X)
            {
                if (_outVel.X > 0)
                {
                    _outVel = -_outVel;
                }
                AnimationMaster.flipX = false;
                FallSpeedX = Math.Abs(FallSpeedX);
                _shootOffset.X = Math.Abs(_shootOffset.X);
            }

        }

        private void FallOnGround()
        {

            //falling
            if (!IsFalling && !OnGround)
            {
                Animation = EnemyAnimationState.Fall;
                
                CurrentYValue = transform.position.Y;
                Debug.log("Current Y Value: " + CurrentYValue);
                Debug.log("Current Transform.position.Y: " + transform.position.Y);
                Movement.Y = -Mathf.sqrt(JumpHeight * Gravity);
                IsFalling = true;
            }
            if (IsFalling)
            {
                //animation = EnemyAnimationState.Fall;
                Movement.X += FallSpeedX * Time.deltaTime;
                Movement.Y += Gravity * Time.deltaTime;
                //movement.Normalize();
                CollisionResult res;
                Mover.move(Movement * FallSpeed * Time.deltaTime, out res);
                if (transform.position.Y >= CurrentYValue)
                {
                    transform.setPosition(new Vector2(transform.position.X, CurrentYValue));
                    Debug.log("Current Y Value after ground hit: " + CurrentYValue);
                    Debug.log("Current Transform.position.Y after ground hit: " + transform.position.Y);
                    IsFalling = false;
                    OnGround = true;
                    SetState(EnemyState.OnGround);
                }
            }
        }

        private void HandleOnGround()
        {
            Movement = Vector2.Zero;
            Animation = EnemyAnimationState.Die;
            if (CurrentLife > 0)
            {
                SetState(EnemyState.GettingUp);

            }
            if (CurrentLife <= 0)
            {
                SetState(EnemyState.Dying);
            }
        }

        private void Getup()
        {
            Animation = EnemyAnimationState.GetUp;
            OnGround = false;
            IsFalling = false;
            ITimer task = Core.schedule(2f, false, timer => this.SetState(EnemyState.Following));
        }

        private void Die()
        {
            if (Alive)
            {
                Alive = false;
                Animation = EnemyAnimationState.Die;
                entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().EnemyDieEffect.Play();
                var task = Core.schedule(3f, false, timer => entity.detachFromScene());
            }
        }
        protected void UpdateLocations()
        {
            var gameScene = entity.scene;
            var player = gameScene.findEntity("player");
            Start = TileMap.worldToTilePosition(entity.transform.position);

            End = TileMap.worldToTilePosition(player.transform.position);
            WeightedSearchPath = WeightedGraph.search(Start, End);
        }

        private bool ShouldFollowPlayer()
        {
            if (WeightedSearchPath != null && WeightedSearchPath.Count() > TargetPositionRange)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private void FollowPlayer()
        {
            //can be hit set true
            if (!CanShoot)
                CanShoot = true;
            if (!_canBeHit)
                _canBeHit = true;
            WalkAnimation();

            if (ShouldFollowPlayer())
            {
                SlowDownBasedOnPlayerAttack();
                TargetPosition = TileMap.tileToWorldPosition(WeightedSearchPath.Last());
                float x = TargetPosition.X - entity.transform.position.X;
                float y = TargetPosition.Y - entity.transform.position.Y;
                x = (float)(x / Math.Sqrt(x * x + y * y));
                y = (float)(y / Math.Sqrt(x * x + y * y));
                Vector2 moveDirection = new Vector2(x, y);
                Movement = moveDirection * MoveSpeed * Time.deltaTime;
                CollisionResult res;
                Mover.move(Movement, out res);
            }
            else if (!ShouldFollowPlayer())
            {
                SetState(EnemyState.Attacking);
            }

        }

        private void GetHit()
        {
            if (_canBeHit)
            {
                _canBeHit = false;
                Animation = EnemyAnimationState.Hit;
                if (WeightedSearchPath != null && WeightedSearchPath.Count() > TargetPositionRange)
                {
                    ITimer newTask = Core.schedule(0.3f, false, newTimer => this.SetState(EnemyState.Following));
                }
                else if (WeightedSearchPath != null && WeightedSearchPath.Count() <= TargetPositionRange)
                {
                    ITimer newTask = Core.schedule(0.3f, false, newTimer => this.SetState(EnemyState.Attacking));
                }
            }


        }

        private void Attack()
        {
            if (!ShouldFollowPlayer())
            {
                if (EnemyCanShootGun)
                {
                    Shoot();
                }
                if (!EnemyCanShootGun)
                {
                    Punch();
                }
            }
            if (ShouldFollowPlayer())
            {
                SetState(EnemyState.Following);
            }

        }
        public virtual void Punch()
        {

            Animation = EnemyAnimationState.Punch;
        }

        public void HandleHits(float inDamage)
        {
            if (EnemyState == EnemyState.Following ||
                EnemyState == EnemyState.Attacking ||
                EnemyState == EnemyState.RunAway)
            {
                CurrentLife -= inDamage;
                if (CurrentLife == 0)
                {
                    SetState(EnemyState.Falling);
                }
                if (CurrentLife != 0 && CurrentLife > TotalLife / 2)
                {
                    entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().EnemyHitEffect.Play();
                    SetState(EnemyState.Hit);
                }
                if (CurrentLife != 0 && CurrentLife <= TotalLife / 2 && !HasFallen)
                {
                    SetState(EnemyState.Falling);
                }
                if (CurrentLife != 0 && CurrentLife <= TotalLife && HasFallen)
                {
                    entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().EnemyHitEffect.Play();
                    SetState(EnemyState.Hit);
                }
            }
        }
        public void MakeBullet(Vector2 position, Vector2 velocity)
        {
            if (entity.scene!= null)
            {
                if (EnemyState == EnemyState.Attacking)
                {
                    var projectile = entity.scene.createEntity("projectile");
                    projectile.position = position;
                    var collider = projectile.addComponent<BoxCollider>();
                    projectile.addComponent(new ProjectileMover());
                    projectile.addComponent(new Utilities.ProjectileController(velocity));
                    Flags.setFlagExclusive(ref collider.collidesWithLayers, (int)Game1.PhysicsLayers.Player);
                    Flags.setFlagExclusive(ref collider.physicsLayer, (int)Game1.PhysicsLayers.EnemyAttacks);
                    projectile.scale = new Vector2(1.5f);
                    var sprite = projectile.addComponent(new Sprite(_bulletTexture));
                    sprite.renderLayer = entity.getComponent<RenderableComponent>().renderLayer;
                    entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().EnemyShootEffect.Play();

                    Core.schedule(1f, false, newTimer => SetShootTrue());
                }
            }
        }

        private void SetShootTrue()
        {
            if (entity.scene!= null)
            {
                CanShoot = true;
            }
        }

        private void Shoot()
        {
            if (entity.scene!= null)
            {
                Animation = EnemyAnimationState.Shoot;
                if (!_canBeHit)
                    _canBeHit = true;
                if (CanShoot)
                {
                    CanShoot = false;
                    Animation = EnemyAnimationState.Shoot;
                    _shootTask = Core.schedule(1f, false, newTimer => MakeBullet(transform.position - _shootOffset, _outVel));
                }
            }

        }

        private void CheckIfStopShooting()
        {
            if (EnemyState != EnemyState.Attacking && _shootTask != null)
            {
                _shootTask.stop();
            }
        }
        protected void CheckIfDead()
        {
            if (CurrentLife == 0)
            {
                if (EnemyState != EnemyState.Falling && EnemyState != EnemyState.OnGround && EnemyState != EnemyState.Dying)
                    SetState(EnemyState.Falling);
                //if (animation != EnemyAnimationState.)
            }
           
        }
    }//End class
}//end namespace
