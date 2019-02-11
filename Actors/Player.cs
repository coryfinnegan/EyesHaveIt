using System;
using System.Collections.Generic;
using Nez;
using Nez.Tiled;
using Nez.Sprites;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez.LibGdxAtlases;
using Nez.TextureAtlases;
using EyesHaveIt.Enums;

namespace EyesHaveIt
{
    public class Player : Component, IUpdatable //ITriggerListener
    {
        public static Player PlayerRef;
        private Mover _mover;
        private ColliderTriggerHelper _triggerHelper;
        public Utilities.ProjectileHitDetector HitDetector { private set; get; }

        private Game1 _gameRef;

        //TiledMapMover _mover;
        private float _moveSpeed = 12f;
        private Vector2 _projectileVelocity = new Vector2(175);
        private Vector2 _velocity;
        private BoxCollider _collider;
        public TiledMapMover.CollisionState CollisionState = new TiledMapMover.CollisionState();
        private Vector2 _boundaries;
        private bool _punching = false;
        public Entity CurrentTarget { private set;  get; }

        private VirtualButton _fireInput;
        private VirtualButton _jumpInput;
        private VirtualButton _endGame;
        private VirtualButton _glassesInput;
        private VirtualIntegerAxis _xAxisInput;
        private VirtualIntegerAxis _yAxisInput;
        private Sprite<PlayerAnimationState> _animationMaster;
        private List<Sprite<PlayerAnimationState>> _sprites;
        private PlayerAnimationState _animation;
        private float _punchDuration = 0.50f;
        private float _punchStartTime;
        private BoxCollider _punchCollider;
        private float _damage = 1f;
        private bool _isColliding = false;
        private int _punchXOffset = 23;
        private int _punchYOffset = -20;
        [Inspectable]
        private float _hitRange = 15;
        public PlayerState PlayerState = PlayerState.Alive;
        public float EndGameZone { set; get; }
        public int Score { set; get; }
        public bool GlassesBool = false;
        private bool _glitchBool = false;
        private Utilities.PostEffectController _postEffectController;
        public float GlassesMeter = 0;
        public bool HasUsedGlasses = false;

        public Player(Game1 game)
        {
            _gameRef = game;
            PlayerRef = this;
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            SetupAnimation();
            SetupCollision();
            entity.scale = new Vector2(2f, 2f);
            SetupInput();
            _postEffectController = entity.scene.findEntity("postEffectController").getComponent<Utilities.PostEffectController>();
        }

        private void SetupAnimation()
        {
            var atlas = entity.scene.content.Load<TextureAtlas>("characters/MainGuy/MainGuyAtlas");
            var walk = atlas.getSpriteAnimation("Walk");
            var idle = atlas.getSpriteAnimation("Idle");
            var die = atlas.getSpriteAnimation("Die");
            die.setLoop(false);
            idle.setFps(5f);
            var punch = atlas.getSpriteAnimation("Punch");
            punch.setFps(15f);
            punch.setLoop(false);

            _animationMaster = entity.addComponent(new Sprite<PlayerAnimationState>(PlayerAnimationState.Idle, idle));
            _animationMaster.addAnimation(PlayerAnimationState.Walk, walk);
            _animationMaster.addAnimation(PlayerAnimationState.Punch, punch);
            _animationMaster.addAnimation(PlayerAnimationState.Die, die);
        }

        private void SetupCollision()
        {
            _collider = new BoxCollider();
            _collider.setWidth(20f);
            _collider.setHeight(70f);
            Flags.setFlagExclusive(ref _collider.collidesWithLayers, (int)Game1.PhysicsLayers.Background);
            Flags.setFlagExclusive(ref _collider.physicsLayer, (int)Game1.PhysicsLayers.Player);
            _punchCollider = new BoxCollider();
            _punchCollider.setWidth(20f);
            _punchCollider.setHeight(5f);
            _punchCollider.setLocalOffset(new Vector2(_punchXOffset, _punchYOffset));
            _punchCollider.isTrigger = true;
            Flags.setFlagExclusive(ref _punchCollider.collidesWithLayers, (int)Game1.PhysicsLayers.PlayerAttacksInactive);
            Flags.setFlagExclusive(ref _punchCollider.physicsLayer, (int)Game1.PhysicsLayers.PlayerAttacks);
            entity.addComponent(_collider);
            entity.addComponent(_punchCollider);
            //punchCollider.setEnabled(false);
            //_collider.isTrigger = true;
            _triggerHelper = new ColliderTriggerHelper(entity);
            HitDetector = new Utilities.ProjectileHitDetector();
            entity.addComponent(HitDetector);


            _mover = entity.addComponent(new Mover());
            Flags.setFlagExclusive(ref _collider.collidesWithLayers, (int)Game1.PhysicsLayers.Background);
            Flags.setFlagExclusive(ref _collider.physicsLayer, (int)Game1.PhysicsLayers.Player);
        }
        public override void onRemovedFromEntity()
        {
            _fireInput.deregister();
            _jumpInput.deregister();
            _endGame.deregister();
            _glassesInput.deregister();
        }

        private void SetupInput()
        {
            // setup input for shooting a gun. we will allow z on the keyboard or a on the gamepad
            _fireInput = new VirtualButton();
            _fireInput.nodes.Add(new Nez.VirtualButton.KeyboardKey(Keys.Space));
            _fireInput.nodes.Add(new Nez.VirtualButton.GamePadButton(0, Buttons.RightShoulder));

            _jumpInput = new VirtualButton();
            //_jumpInput.nodes.Add(new Nez.VirtualButton.KeyboardKey(Keys.Space));
            _jumpInput.nodes.Add(new Nez.VirtualButton.GamePadButton(0, Buttons.A));

            _glassesInput = new VirtualButton();
            _glassesInput.nodes.Add(new VirtualButton.KeyboardKey(Keys.E));


            _endGame = new VirtualButton();
            _endGame.nodes.Add(new Nez.VirtualButton.KeyboardKey(Keys.H));

            // horizontal input from dpad, left stick or keyboard left/right
            _xAxisInput = new VirtualIntegerAxis();
            _xAxisInput.nodes.Add(new Nez.VirtualAxis.GamePadDpadLeftRight());
            _xAxisInput.nodes.Add(new Nez.VirtualAxis.GamePadLeftStickX());
            _xAxisInput.nodes.Add(new Nez.VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right));

          
            // vertical input from dpad, left stick or keyboard up/down
            _yAxisInput = new VirtualIntegerAxis();
            _yAxisInput.nodes.Add(new Nez.VirtualAxis.GamePadDpadUpDown());
            _yAxisInput.nodes.Add(new Nez.VirtualAxis.GamePadLeftStickY());
            _yAxisInput.nodes.Add(new Nez.VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Up, Keys.Down));
        
        }
        public void SetState(PlayerState inState)
        {
            Debug.log("Setting state for player on to: " + inState);
            PlayerState = inState;
        }
        void IUpdatable.update()
        {

            EndGameIfReachedEndOfMap();
            PlayerStateController();
        }

        private void PlayerStateController()
        {
            switch (PlayerState)
            {
                case PlayerState.Alive:
                    HandleBeingAlive();
                    break;
                case PlayerState.Dead:
                    Die();
                    break;
                case PlayerState.Hit:
                    break;
            }
        }

        private void HandleBeingAlive()
        {
            MoveCharacter();
            ControlPunching();
            HandleRenderLayers();
            PutOnGlasses();
            ManagePixelGlitch();
            KillPlayerFromGlasses();
        }

        private void HandleRenderLayers()
        {
            entity.getComponent<RenderableComponent>().renderLayer = -(int)transform.position.Y;
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

        private void ControlPunching()
        {
            _isColliding = false;
            float totalLife;
            if (_fireInput.isPressed)
            {

                _animation = PlayerAnimationState.Punch;
                _punching = true;
                _punchStartTime = Time.time;
                _animationMaster.play(_animation);
                entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().PlayerPunch.Play();
                //punchCollider.setEnabled(true);
                //Flags.setFlagExclusive(ref punchCollider.collidesWithLayers, (int)Game1.PhysicsLayers.Enemy);
                var neighborColliders = Physics.boxcastBroadphaseExcludingSelf(_punchCollider);
                
                foreach (var collider in neighborColliders)
                {
                    if (_punchCollider.overlaps(collider) && collider.entity.getComponent<Utilities.HitController>()!= null && CheckYValue(collider))
                    {
                        CheckYValue(collider);
                        var hitCollider = collider.entity.getComponent<Utilities.HitController>();
                        totalLife = hitCollider.GetLife();
                       
                        if (totalLife> 0)
                        {
                            if (_isColliding)
                            {
                                return;
                            }
                            else
                            {
                                Debug.log(this.ToString() + " Damaging:  " + collider.entity.name);
                                _isColliding = true;
                                collider.entity.getComponent<Utilities.HitController>().DoDamage(_damage);
                                CurrentTarget = collider.entity;
                                Score += 100;
                                
                            }
                        }
                        else if(totalLife == 0)
                        {
                            CurrentTarget = null;
                            collider.unregisterColliderWithPhysicsSystem();
                        }
                    }
                }
            }
            if ((_animationMaster.isAnimationPlaying(PlayerAnimationState.Punch) && ((Time.time - _punchStartTime) >= _punchDuration)))
            {
                _punching = false;
                _punchStartTime = 0f;
                _animationMaster.pause();
                _animation = PlayerAnimationState.Idle;
                _animationMaster.play(_animation);
            }
        }

        private void MoveCharacter()
        {
            if (!_punching)
            {
                _animation = PlayerAnimationState.Idle;
                var moveDir = new Vector2(_xAxisInput.value, _yAxisInput.value);
                if (moveDir.X < 0)
                {
                    _animation = PlayerAnimationState.Walk;
                    _animationMaster.flipX = true;
                    _velocity.X = -_moveSpeed;
                    _punchCollider.setLocalOffset(new Vector2(-_punchXOffset, _punchYOffset));
                }
                else if (moveDir.X > 0)
                {
                    _animation = PlayerAnimationState.Walk;
                    _animationMaster.flipX = false;
                    _velocity.X = _moveSpeed;
                    _punchCollider.setLocalOffset(new Vector2(_punchXOffset, _punchYOffset));
                }
                if (moveDir.Y < 0)
                {
                    _animation = PlayerAnimationState.Walk;
                    _velocity.Y = -_moveSpeed;
                }
                else if (moveDir.Y > 0)
                {
                    _animation = PlayerAnimationState.Walk;
                    _velocity.Y = _moveSpeed;
                }
                if (moveDir != Vector2.Zero)
                {
                    if (!_animationMaster.isAnimationPlaying(_animation))
                    {
                        _animationMaster.play(_animation);
                    }
                    var movement = _velocity * _moveSpeed * Time.deltaTime;

                    CollisionResult res;
                    _mover.move(movement, out res);
                    _triggerHelper.update();
                }
                else
                {
                    if (!_animationMaster.isAnimationPlaying(PlayerAnimationState.Idle) && !_animationMaster.isAnimationPlaying(PlayerAnimationState.Punch))
                    {
                        _animation = PlayerAnimationState.Idle;
                        _animationMaster.play(_animation);
                    }
                    _velocity.X = 0;
                    _velocity.Y = 0;
                }
            }
            
        }

        private void Die()
        {
            _animation = PlayerAnimationState.Die;
            if (!_animationMaster.isAnimationPlaying(_animation))
            {
                _animationMaster.play(_animation);
                
            }
            Core.schedule(5f, false, newTimer => Game1.endGameMenu("lose"));
        }
        public bool IsPunching()
        {
            return _punching;
        }

        private void EndGameIfReachedEndOfMap()
        {
            if (transform.position.X >= EndGameZone && GetEnemyCount() <=0) 
            {
                Core.schedule(5f, false, newTimer => Game1.endGameMenu("win"));
            }
        }

        private int GetEnemyCount()
        {
            var count = entity.scene.findEntitiesWithTag((int)Enums.Tags.Enemy).Count;
            return count;
        }

        private void PutOnGlasses()
        {
            if (_glassesInput.isPressed)
            {
                HasUsedGlasses = true;
                if (GlassesBool)
                {
                    GlassesBool = false;
                    _postEffectController.RemoveGreyScale();
                    _damage = 1f;         
                    if (_glitchBool)
                    {
                        _glitchBool = false;
                        _postEffectController.RemovePixelGlitch();
                    }         
                }
                else
                {
                    _damage = 4f;
                    GlassesBool = true;
                    _postEffectController.AddGreyScale();

                }
            }
        }

        private void ManagePixelGlitch()
        {
            if (GlassesBool)
            {  
                IncreaseGlassesMeter();   

            }
            if (!GlassesBool)
            {
                DecreaseGlassesMeter();
            }

        }

        private void DecreaseGlassesMeter()
        {
            float decreaseAmount = -0.01f;
            GlassesMeter += decreaseAmount;
            Mathf.clamp(GlassesMeter, 0, 146);
        }

        private void AddPixelGlitch()
        {
            if (GlassesMeter >= 10 && !_glitchBool)
            {
                _glitchBool = true;
                _postEffectController.AddPixelGlitch();
                _postEffectController.PixelGlitchPostProcessor.horizontalOffset = 0.1f;

            }
        }

        private void IncreaseGlassesMeter()
        {
            if (GlassesBool)
            {
                GlassesMeter += 0.1f;
                AddPixelGlitch();
                if (_glitchBool)
                    IncreasePixelGlitch();
            }
        }

        private void IncreasePixelGlitch()
        {
            var offsetIncrease = 0.1f;
            if ((int)GlassesMeter % 2 == 0)
            {
                _postEffectController.IncreasePixelGlitch(offsetIncrease);
            }
        }

        private void KillPlayerFromGlasses()
        {
            if (GlassesMeter >= 145 && PlayerState != PlayerState.Dead)
            {
                PlayerState = PlayerState.Dead;
            }
        }
    }
}
