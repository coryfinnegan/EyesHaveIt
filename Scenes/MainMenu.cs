using Nez;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;
using Nez.UI;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;

namespace EyesHaveIt.Scenes
{
    public class MainMenu : Scene
    {
        public const int SCREEN_SPACE_RENDER_LAYER = 999;
        public const int SCANLINE_RENDER_LAYER = 50;

        public MainMenu() : base()
        {
            clearColor = Color.Black;
            this.addRenderer(new DefaultRenderer());

            var canvas = createEntity("ui").addComponent(new UICanvas());
            var backgroundTexture = content.Load<Texture2D>("backgrounds/titleScreen");
            var background = createEntity("titleScreenBackground", new Vector2(Screen.width / 3f, Screen.height / 2f));
            background.addComponent(new Sprite(backgroundTexture));
            canvas.addComponent<Sprite>(new Sprite(backgroundTexture));
            var table = canvas.stage.addElement(new Table());
            //table.setDebug(true);
            table.setFillParent(true).center().bottom();
            var mainMenuFont = content.Load<BitmapFont>("fonts/futuraDouble");
            var topButtonStyle = new TextButtonStyle(new PrimitiveDrawable(Color.Transparent, 10f), new PrimitiveDrawable(Color.Transparent, 10f), new PrimitiveDrawable(Color.Transparent, 10f), mainMenuFont)
            {
                downFontColor = Color.Red,
                overFontColor = Color.LightGray
            };
            TextButton playButton = new TextButton("CONSUME", topButtonStyle);
            TextButton quitButton = new TextButton("SLEEP", topButtonStyle);
            table.add(playButton).setFillX().setMinHeight(10).setMinWidth(100).getElement<Button>().onClicked += Game1.startLevelOne;
            table.row().setPadBottom(100);
            table.add(quitButton).setFillX().setMinHeight(10).setMinWidth(100).getElement<Button>().onClicked += Game1.quitGame;
            addPostProcessor(new VignettePostProcessor(999));
            addPostProcessor(new ScanlinesPostProcessor(0));

            initialize();
        }
        public override void initialize()
        {
            //called twice
        }



    }
}
