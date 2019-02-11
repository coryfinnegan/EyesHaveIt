using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Nez;
using Nez.BitmapFonts;
using Nez.Sprites;
using Nez.Tiled;
using Nez.UI;
using System.Collections.Generic;


namespace EyesHaveIt
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Core
    {
        public enum PhysicsLayers
        {
            Background,
            Player,
            Enemy, 
            PlayerAttacks,
            PlayerAttacksInactive, 
            EnemyAttacks
        }
        public static Game1 gameRef;
        private Scene.SceneResolutionPolicy policy;
        public const int SCREEN_SPACE_RENDER_LAYER = 999;
        public UICanvas canvas;
        private Table table;
        private BitmapFont mainMenuFont;
        private List<Button> _sceneButtons = new List<Button>();
        private Scenes.MainMenu mainMenu;

        

        public Game1() : base()
        {
            policy = Scene.SceneResolutionPolicy.FixedHeightPixelPerfect;
            Window.AllowUserResizing = true;
            gameRef = this;
            debugRenderEnabled = false;
        }

        protected override void Initialize()
        {
            base.Initialize();
            scene = Scene.createWithDefaultRenderer(Color.Black);
            mainMenu = new Scenes.MainMenu();
            base.Update(new GameTime());
            base.Draw(new GameTime());
            Screen.setSize(800, 600);
            startMainMenu();
        }
        public void addEntities()
        {
            
        }

        private void startMainMenu()
        {
            scene = mainMenu;
        }
        public static void startLevelOne(Button button)
        {
            Scene levelOne = new Scenes.LevelOne();
            scene = levelOne;
        }
        public static void quitGame(Button button)
        {
            gameRef.Exit();
        }
        public static void endGameMenu(string gameResult)
        {
            Scene endGame = new Scenes.EndGameMenu(gameResult);
            scene = endGame;
        }
        public static void changeLevel(Scene sceneName)
        {

        }


    }
}
