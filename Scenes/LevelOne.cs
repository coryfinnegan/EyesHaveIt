using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Tiled;
using Nez.UI;



namespace EyesHaveIt.Scenes
{
    internal class LevelOne : Scene
    {
        private TiledMap backgroundTileMap;
        private Entity enemy1;
        private Entity player;
        private Player playerObject;
        private Utilities.UiController uiController;
        public const int SCREEN_SPACE_RENDER_LAYER = 999;
        public UICanvas canvas;
        private Table _table;
        private ScreenSpaceRenderer _screenSpaceRenderer;
        public ProgressBar bar;
        public TiledObject endGameZone { private set; get; }
        public Utilities.AudioController audioController { private set; get; }
        public Utilities.EnemySpawnController enemySpawnController { private set; get; }
        public Utilities.PostEffectController postEffectController { private set; get;  }


        public LevelOne() : base()
        {
            clearColor = Color.Black;
            _screenSpaceRenderer = new ScreenSpaceRenderer(100, SCREEN_SPACE_RENDER_LAYER);
            _screenSpaceRenderer.shouldDebugRender = false;
            this.addRenderer(_screenSpaceRenderer);
            
            this.addRenderer(new DefaultRenderer());
            var postEffectControllerEntity = createEntity("postEffectController");
            postEffectController = postEffectControllerEntity.addComponent(new Utilities.PostEffectController());
            
            postEffectController.AddScanlines();
            addPostProcessor(new VignettePostProcessor(0));



            //uiController = createEntity("ui").addComponent(new Utilities.UIController());


            var background = createEntity("background");
            backgroundTileMap = content.Load<Nez.Tiled.TiledMap>("maps/map2");
            //Scene.setDefaultDesignResolution(backgroundTileMap.width, backgroundTileMap.height, policy);

            var objectLayer = backgroundTileMap.getObjectGroup("objects");

            var tiledMapComponent = background.addComponent(new TiledMapComponent(backgroundTileMap, "collisionLayer"));
            tiledMapComponent.setLayersToRender(new string[] { "collisionLayer", "floor", "Sky", "buildings", "boxes", "city" });
            background.addComponent(new CameraBounds(new Vector2(backgroundTileMap.tileWidth, backgroundTileMap.tileWidth), new Vector2(backgroundTileMap.tileWidth * (backgroundTileMap.width - 1), backgroundTileMap.tileWidth * (backgroundTileMap.height - 1))));
            tiledMapComponent.renderLayer = 100;
            setupPlayer(objectLayer);
            //setupEnemies(objectLayer);
            setupEnemySpawn();
            setupUI();
            setupAudio();


            initialize();
        }

        private void setupGlassesEffects()
        {

        }

        private void setupEnemySpawn()
        {
            enemySpawnController = new Utilities.EnemySpawnController(backgroundTileMap);
            createEntity("enemySpawnController").addComponent(enemySpawnController);
        }

        private void setupPlayer(TiledObjectGroup objectLayer)
        {
            var spawn = objectLayer.objectWithName("spawn");
            player = createEntity("player");
            playerObject = player.addComponent(new Player(Game1.gameRef));
            player.transform.setPosition(spawn.x, spawn.y);
            camera.addComponent(new FollowCamera(player));
            endGameZone = objectLayer.objectWithName("endGame");
            playerObject.EndGameZone = endGameZone.x;
            
        }
        public override void initialize()
        {
            //Dont use - it is called twice
        }

        private void setupEnemies(Nez.Tiled.TiledObjectGroup mapObjectLayer)
        {
            
            var enemy1Spawn = mapObjectLayer.objectWithName("enemySpawn1");
            enemy1 = createEntity("enemy1");

            //turned off for testing
            //enemy1.setTag((int)Tags.gameEntity);
            enemy1.addComponent(new Enemy("EnemyAgent", 4, backgroundTileMap, true, 4));
            //enemy1.addComponent(new BoxCollider());
            enemy1.scale = new Vector2(2f, 2f);
            enemy1.transform.position = new Vector2(enemy1Spawn.x, enemy1Spawn.y);

            var punk1Spawn = mapObjectLayer.objectWithName("punkSpawn1");
            var punk1 = createEntity("punk1");

            punk1.addComponent(new Actors.Punk("Punk", 4, backgroundTileMap, false, 4));
            punk1.scale = new Vector2(2);
            punk1.transform.position = new Vector2(punk1Spawn.x, punk1Spawn.y);

        }

        private void setupUI()
        {
            uiController = new Utilities.UiController();
            createEntity("ui").addComponent(uiController);
        }

        private void setupAudio()
        {
            audioController = new Utilities.AudioController();
            createEntity("audio").addComponent(audioController);
        }

    }
}
