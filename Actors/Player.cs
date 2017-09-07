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

namespace EyesHaveIt
{
    public class Player : Component, IUpdatable //ITriggerListener
    {

        enum Animations
        {
            Walk,
            Idle,
            Punch,
            Die
            
        }
        public enum PlayerState
        {
            Alive, 
            Dead, 
            Hit
        }
        public static Player playerRef;
        Mover _mover;
        ColliderTriggerHelper triggerHelper;
        public Utilities.ProjectileHitDetector hitDetector { private set; get; }
        Game1 gameRef;
        //TiledMapMover _mover;
        float _moveSpeed = 12f;
        Vector2 _projectileVelocity = new Vector2(175);
        Vector2 _velocity;
        BoxCollider _collider;
        public TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
        Vector2 boundaries;
        bool punching = false;
        public Entity currentTarget { private set;  get; }
        VirtualButton _fireInput;
        VirtualButton _jumpInput;
        VirtualButton _endGame;
        VirtualButton _glassesInput;
        VirtualIntegerAxis _xAxisInput;
        VirtualIntegerAxis _yAxisInput;
        Sprite<Animations> _animationMaster;
        List<Sprite<Animations>> sprites;
        Animations animation;

        float punchDuration = 0.50f;
        float punchStartTime;
        BoxCollider punchCollider;
        float damage = 1f;
        bool isColliding = false;
        int punchXOffset = 23;
        int punchYOffset = -20;
        [Inspectable]
        float hitRange = 15;
        public PlayerState playerState = PlayerState.Alive;
        public float endGameZone { set; get; }
        public int score { set; get; }
        public bool glassesBool = false;
        bool glitchBool = false;
        Utilities.PostEffectController postEffectController;
        public float glassesMeter = 0;
        public bool hasUsedGlasses = false;

        public Player(Game1 game)
        {
            gameRef = game;
            playerRef = this;
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            setupAnimation();
            setupCollision();
            entity.scale = new Vector2(2f, 2f);
            setupInput();
            postEffectController = entity.scene.findEntity("postEffectController").getComponent<Utilities.PostEffectController>();
        }
        void setupAnimation()
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

            _animationMaster = entity.addComponent(new Sprite<Animations>(Animations.Idle, idle));
            _animationMaster.addAnimation(Animations.Walk, walk);
            _animationMaster.addAnimation(Animations.Punch, punch);
            _animationMaster.addAnimation(Animations.Die, die);
        }
        void setupCollision()
        {
            _collider = new BoxCollider();
            _collider.setWidth(20f);
            _collider.setHeight(70f);
            Flags.setFlagExclusive(ref _collider.collidesWithLayers, (int)Game1.PhysicsLayers.Background);
            Flags.setFlagExclusive(ref _collider.physicsLayer, (int)Game1.PhysicsLayers.Player);
            punchCollider = new BoxCollider();
            punchCollider.setWidth(20f);
            punchCollider.setHeight(5f);
            punchCollider.setLocalOffset(new Vector2(punchXOffset, punchYOffset));
            punchCollider.isTrigger = true;
            Flags.setFlagExclusive(ref punchCollider.collidesWithLayers, (int)Game1.PhysicsLayers.PlayerAttacksInactive);
            Flags.setFlagExclusive(ref punchCollider.physicsLayer, (int)Game1.PhysicsLayers.PlayerAttacks);
            entity.addComponent(_collider);
            entity.addComponent(punchCollider);
            //punchCollider.setEnabled(false);
            //_collider.isTrigger = true;
            triggerHelper = new ColliderTriggerHelper(entity);
            hitDetector = new Utilities.ProjectileHitDetector();
            entity.addComponent(hitDetector);


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
        void setupInput()
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
        public void setState(PlayerState inState)
        {
            Debug.log("Setting state for player on to: " + inState);
            playerState = inState;
        }
        void IUpdatable.update()
        {

            checkIfEndGame();
            playerStateController();
        }
        void playerStateController()
        {
            switch (playerState)
            {
                case PlayerState.Alive:
                    handleBeingAlive();
                    break;
                case PlayerState.Dead:
                    die();
                    break;
                case PlayerState.Hit:
                    break;
            }
        }        
        void handleBeingAlive()
        {
            handleMovement();
            handlePunching();
            handleRenderLayers();
            putOnGlasses();
            managePixelGlitch();
            killPlayerFromGlasses();
        }
        void handleRenderLayers()
        {
            entity.getComponent<RenderableComponent>().renderLayer = -(int)transform.position.Y;
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
        void handlePunching()
        {
            isColliding = false;
            float totalLife;
            if (_fireInput.isPressed)
            {

                animation = Animations.Punch;
                punching = true;
                punchStartTime = Time.time;
                _animationMaster.play(animation);
                entity.scene.findEntity("audio").getComponent<Utilities.AudioController>().playerPunch.Play();
                //punchCollider.setEnabled(true);
                //Flags.setFlagExclusive(ref punchCollider.collidesWithLayers, (int)Game1.PhysicsLayers.Enemy);
                var neighborColliders = Physics.boxcastBroadphaseExcludingSelf(punchCollider);
                
                foreach (var collider in neighborColliders)
                {
                    if (punchCollider.overlaps(collider) && collider.entity.getComponent<Utilities.HitController>()!= null && checkYValue(collider))
                    {
                        checkYValue(collider);
                        var hitCollider = collider.entity.getComponent<Utilities.HitController>();
                        totalLife = hitCollider.getLife();
                       
                        if (totalLife> 0)
                        {
                            if (isColliding)
                            {
                                return;
                            }
                            else
                            {
                                Debug.log(this.ToString() + " Damaging:  " + collider.entity.name);
                                isColliding = true;
                                collider.entity.getComponent<Utilities.HitController>().doDamage(damage);
                                currentTarget = collider.entity;
                                score += 100;
                                
                            }
                        }
                        else if(totalLife == 0)
                        {
                            currentTarget = null;
                            collider.unregisterColliderWithPhysicsSystem();
                        }
                    }
                }
            }
            if ((_animationMaster.isAnimationPlaying(Animations.Punch) && ((Time.time - punchStartTime) >= punchDuration)))
            {
                punching = false;
                punchStartTime = 0f;
                _animationMaster.isPlaying = false;
                animation = Animations.Idle;
                _animationMaster.play(animation);
            }
        }

        void handleMovement()
        {
            if (!punching)
            {
                animation = Animations.Idle;
                var moveDir = new Vector2(_xAxisInput.value, _yAxisInput.value);
                if (moveDir.X < 0)
                {
                    animation = Animations.Walk;
                    _animationMaster.flipX = true;
                    _velocity.X = -_moveSpeed;
                    punchCollider.setLocalOffset(new Vector2(-punchXOffset, punchYOffset));
                }
                else if (moveDir.X > 0)
                {
                    animation = Animations.Walk;
                    _animationMaster.flipX = false;
                    _velocity.X = _moveSpeed;
                    punchCollider.setLocalOffset(new Vector2(punchXOffset, punchYOffset));
                }
                if (moveDir.Y < 0)
                {
                    animation = Animations.Walk;
                    _velocity.Y = -_moveSpeed;
                }
                else if (moveDir.Y > 0)
                {
                    animation = Animations.Walk;
                    _velocity.Y = _moveSpeed;
                }
                if (moveDir != Vector2.Zero)
                {
                    if (!_animationMaster.isAnimationPlaying(animation))
                    {
                        _animationMaster.play(animation);
                    }
                    var movement = _velocity * _moveSpeed * Time.deltaTime;

                    CollisionResult res;
                    _mover.move(movement, out res);
                    triggerHelper.update();
                }
                else
                {
                    if (!_animationMaster.isAnimationPlaying(Animations.Idle) && !_animationMaster.isAnimationPlaying(Animations.Punch))
                    {
                        animation = Animations.Idle;
                        _animationMaster.play(animation);
                    }
                    _velocity.X = 0;
                    _velocity.Y = 0;
                }
            }
            
        }
        
       void die()
        {
            animation = Animations.Die;
            if (!_animationMaster.isAnimationPlaying(animation))
            {
                _animationMaster.play(animation);
                
            }
            Core.schedule(5f, false, newTimer => Game1.endGameMenu("lose"));
        }
        public bool getPunching()
        {
            return punching;
        }
        void checkIfEndGame()
        {
            if (transform.position.X >= endGameZone && getEnemyCount() <=0) 
            {
                Core.schedule(5f, false, newTimer => Game1.endGameMenu("win"));
            }
        }
        int getEnemyCount()
        {
            int count = entity.scene.findObjectsOfType<Enemy>().Count;
            return count;
        }
        void putOnGlasses()
        {
            if (_glassesInput.isPressed)
            {
                hasUsedGlasses = true;
                if (glassesBool)
                {
                    glassesBool = false;
                    postEffectController.removeGreyScale();
                    damage = 1f;         
                    if (glitchBool)
                    {
                        glitchBool = false;
                        postEffectController.removePixelGlitch();
                    }         
                }
                else
                {
                    damage = 4f;
                    glassesBool = true;
                    postEffectController.addGreyScale();

                }
            }
        }
        void managePixelGlitch()
        {
            if (glassesBool)
            {  
                increaseGlassesMeter();   

            }
            if (!glassesBool)
            {
                decreaseGlassesMeter();
            }

        }
        void decreaseGlassesMeter()
        {
            float decreaseAmount = -0.01f;
            glassesMeter += decreaseAmount;
            Mathf.clamp(glassesMeter, 0, 146);
        }
        void addPixelGlitch()
        {
            if (glassesMeter >= 10 && !glitchBool)
            {
                glitchBool = true;
                postEffectController.addPixelGlitch();
                postEffectController.pixelGlitchPostProcessor.horizontalOffset = 0.1f;

            }
        }
        void increaseGlassesMeter()
        {
            if (glassesBool)
            {
                glassesMeter += 0.1f;
                addPixelGlitch();
                if (glitchBool)
                    increasePixelGlitch();
            }
        }
        void increasePixelGlitch()
        {
            var offsetIncrease = 0.1f;
            if ((int)glassesMeter % 2 == 0)
            {
                postEffectController.increasePixelGlitch(offsetIncrease);
            }
        }
        void killPlayerFromGlasses()
        {
            if (glassesMeter >= 145 && playerState != PlayerState.Dead)
            {
                playerState = PlayerState.Dead;
            }
        }
    }
}
