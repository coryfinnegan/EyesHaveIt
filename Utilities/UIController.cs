using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez.UI;
using Nez;
using Microsoft.Xna.Framework;

namespace EyesHaveIt.Utilities
{
    class UIController : Component, IUpdatable
    {
        public const int SCREEN_SPACE_RENDER_LAYER = 999;
        UICanvas canvas;
        Table table;
        Skin skin;
        Button healthBar;
        Button enemyHealthBar;
        Button glassesBar;
        ScreenSpaceRenderer screenSpaceRender;
        bool enemyEngaged = false;
        int score = 0;
        Entity scoreEntity;
        Entity tutorial;

        public UIController()
        {
            screenSpaceRender = new ScreenSpaceRenderer(100, SCREEN_SPACE_RENDER_LAYER);
            //Scene.final

        }
        public override void onAddedToEntity()
        {
            canvas = entity.scene.createEntity("canvas").addComponent(new UICanvas());
            canvas.isFullScreen = true;
            canvas.renderLayer = SCREEN_SPACE_RENDER_LAYER;
            table = canvas.stage.addElement(new Table());
            table.setFillParent(true).left().top();
            table.row().setPadTop(10);
            var healthButtonStyle = new ButtonStyle(new PrimitiveDrawable(Color.Green, 10f, 12f), new PrimitiveDrawable(Color.Green, 12f, 10f), new PrimitiveDrawable(Color.Green, 10f, 12f))
            {
            };
            var enemyHealthButtonStyle = new ButtonStyle(new PrimitiveDrawable(Color.Red, 10f, 10f), new PrimitiveDrawable(Color.Red, 10f, 10f), new PrimitiveDrawable(Color.Red, 10f, 10f))
            {
            };
            healthBar = new Button(healthButtonStyle);
            glassesBar = new Button(enemyHealthButtonStyle);
            table.add(healthBar).setMinWidth(20).setPadLeft(5f).setPadTop(12f);
            table.add(glassesBar).setMinWidth(20).setMinHeight(0).right().setPadLeft(475).setPadTop(12f);
            addText("Player Health", 6, 1);
            addText("Eyes", 500, 1);
            setupScore();
            showTutorial();
        }
        void IUpdatable.update()
        {

            updatePlayerHealth();
            updatePlayerEyes();
            updateScore();
            removeTutorial();
        }
        void updatePlayerHealth()
        {
            if (entity.scene!= null)
            {
                healthBar.setWidth(Player.playerRef.hitDetector.hitsUntilDead * 30);

            }
        }
        void updatePlayerEyes()
        {
            if (entity.scene!= null)
            {
                glassesBar.setWidth(Mathf.clamp(Player.playerRef.glassesMeter * 2, 0, 290));
            }
        }
        void addText(string text, int xOffset, int yOffset)
        {
            var textEntity = entity.scene.createEntity("text");
            textEntity.addComponent(new Text(Graphics.instance.bitmapFont, text, new Vector2(xOffset, yOffset), Color.White))
                .setRenderLayer(SCREEN_SPACE_RENDER_LAYER);
        }
        void addEnemyHealth()
        {
            if (Player.playerRef.currentTarget != null && !enemyEngaged)
            {
                table.add(enemyHealthBar).setMinWidth(20).setMinHeight(0).right().setPadLeft(475).setPadTop(12f);
                addText("Enemy", 500, 1);
                enemyEngaged = true;
            }
        }
        void updateEnemyHealth()
        {
             if (Player.playerRef.currentTarget != null && enemyEngaged)
             {
                enemyHealthBar.setWidth(Player.playerRef.currentTarget.getComponent<HitController>().getLife() * 30);
             }
        }
        void removeEnemyHealth()
        {
            if (Player.playerRef.currentTarget == null)
            {
                enemyEngaged = false;
                table.removeElement(enemyHealthBar);
            }
        }
        void showAndRemoveEnemyHealth()
        {
            if (entity.scene != null)
            {
                addEnemyHealth();
                removeEnemyHealth();
                updateEnemyHealth();
                
            }
        }
        void setupScore()
        {
            var scoreString = "Score: " + score;
            scoreEntity = entity.scene.createEntity("score");
            scoreEntity.addComponent(new Text(Graphics.instance.bitmapFont, scoreString, new Vector2(Screen.width / 2.4f, 1), Color.White))
                .setRenderLayer(SCREEN_SPACE_RENDER_LAYER);
           scoreEntity.scale = new Vector2(2f);
        }
        void updateScore()
        {
            score = Player.playerRef.score;
            scoreEntity.getComponent<Text>().setText("Score: " + score);
        }
        void showTutorial()
        {
            tutorial = entity.scene.createEntity("tutorial");
            tutorial.addComponent(new Text(Graphics.instance.bitmapFont, "Press E to See...", new Vector2(20, Screen.height - 20f), Color.White)).setRenderLayer(SCREEN_SPACE_RENDER_LAYER);
            tutorial.scale = new Vector2(2);
        }
        void removeTutorial()
        {
            if (Player.playerRef.hasUsedGlasses && tutorial!= null)
            {
                tutorial.getComponent<Text>().setText("");
            }
        }

    }//end class
}//end namespace
