using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using Nez.Sprites;
using Nez.UI;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework.Graphics;

namespace EyesHaveIt.Scenes
{
    class EndGameMenu : Scene
    {
        public EndGameMenu(String gameResult) : base()
        {
            clearColor = Color.Black;
            this.addRenderer(new DefaultRenderer());
            addPostProcessor(new ScanlinesPostProcessor(0));
            addPostProcessor(new VignettePostProcessor(0));
            var canvas = createEntity("ui").addComponent(new UICanvas());
            canvas.isFullScreen = true;
            canvas.renderLayer = Game1.SCREEN_SPACE_RENDER_LAYER;
            var backgroundTexture = content.Load<Texture2D>("backgrounds/"+gameResult+"Screen");
            var background = createEntity("titleScreenBackground", new Vector2(Screen.width / 2f, Screen.height / 1.5f));
            background.addComponent(new Sprite(backgroundTexture));
            var table = canvas.stage.addElement(new Table());
            table.setFillParent(true).center().bottom();
            var mainMenuFont = content.Load<BitmapFont>("fonts/futuraDouble");
            var topButtonStyle = new TextButtonStyle(new PrimitiveDrawable(Color.Transparent, 10f), new PrimitiveDrawable(Color.Transparent, 10f), new PrimitiveDrawable(Color.Transparent, 10f), mainMenuFont)
            {
                downFontColor = Color.Red,
                overFontColor = Color.LightGray
            };
            TextButton restartButton = new TextButton("CONSUME MORE", topButtonStyle);
            TextButton quitButton = new TextButton("STAY ASLEEP", topButtonStyle);
            table.add(restartButton).setFillX().setMinHeight(10).setMinWidth(100).getElement<Button>().onClicked += Game1.startLevelOne;
            table.row().setPadBottom(100);
            table.add(quitButton).setFillX().setMinHeight(10).setMinWidth(100).getElement<Button>().onClicked += Game1.quitGame;
            initialize();
        }
        public override void initialize()
        {
            base.initialize();

        }
    }
}
