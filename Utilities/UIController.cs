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
    internal class UiController : Component, IUpdatable
    {
        public const int ScreenSpaceRenderLayer = 999;
        private UICanvas _canvas;
        private Table _table;
        private Skin _skin;
        private Button _healthBar;
        private Button _enemyHealthBar;
        private Button _glassesBar;
        private ScreenSpaceRenderer _screenSpaceRender;
        private bool _enemyEngaged = false;
        private int _score = 0;
        private Entity _scoreEntity;
        private Entity _tutorial;

        public UiController()
        {
            _screenSpaceRender = new ScreenSpaceRenderer(100, ScreenSpaceRenderLayer);
            //Scene.final

        }
        public override void onAddedToEntity()
        {
            _canvas = entity.scene.createEntity("canvas").addComponent(new UICanvas());
            _canvas.isFullScreen = true;
            _canvas.renderLayer = ScreenSpaceRenderLayer;
            _table = _canvas.stage.addElement(new Table());
            _table.setFillParent(true).left().top();
            _table.row().setPadTop(10);
            var healthButtonStyle = new ButtonStyle(new PrimitiveDrawable(Color.Green, 10f, 12f), new PrimitiveDrawable(Color.Green, 12f, 10f), new PrimitiveDrawable(Color.Green, 10f, 12f))
            {
            };
            var enemyHealthButtonStyle = new ButtonStyle(new PrimitiveDrawable(Color.Red, 10f, 10f), new PrimitiveDrawable(Color.Red, 10f, 10f), new PrimitiveDrawable(Color.Red, 10f, 10f))
            {
            };
            _healthBar = new Button(healthButtonStyle);
            _glassesBar = new Button(enemyHealthButtonStyle);
            _table.add(_healthBar).setMinWidth(20).setPadLeft(5f).setPadTop(12f);
            _table.add(_glassesBar).setMinWidth(20).setMinHeight(0).right().setPadLeft(475).setPadTop(12f);
            AddText("Player Health", 6, 1);
            AddText("Eyes", 500, 1);
            SetupScore();
            ShowTutorial();
        }
        void IUpdatable.update()
        {

            UpdatePlayerHealth();
            UpdatePlayerEyes();
            UpdateScore();
            RemoveTutorial();
        }

        private void UpdatePlayerHealth()
        {
            if (entity.scene!= null)
            {
                _healthBar.setWidth(Player.PlayerRef.HitDetector.HitsUntilDead * 30);

            }
        }

        private void UpdatePlayerEyes()
        {
            if (entity.scene!= null)
            {
                _glassesBar.setWidth(Mathf.clamp(Player.PlayerRef.GlassesMeter * 2, 0, 290));
            }
        }

        private void AddText(string text, int xOffset, int yOffset)
        {
            var textEntity = entity.scene.createEntity("text");
            textEntity.addComponent(new Text(Graphics.instance.bitmapFont, text, new Vector2(xOffset, yOffset), Color.White))
                .setRenderLayer(ScreenSpaceRenderLayer);
        }

        private void AddEnemyHealth()
        {
            if (Player.PlayerRef.CurrentTarget != null && !_enemyEngaged)
            {
                _table.add(_enemyHealthBar).setMinWidth(20).setMinHeight(0).right().setPadLeft(475).setPadTop(12f);
                AddText("Enemy", 500, 1);
                _enemyEngaged = true;
            }
        }

        private void UpdateEnemyHealth()
        {
             if (Player.PlayerRef.CurrentTarget != null && _enemyEngaged)
             {
                _enemyHealthBar.setWidth(Player.PlayerRef.CurrentTarget.getComponent<HitController>().GetLife() * 30);
             }
        }

        private void RemoveEnemyHealth()
        {
            if (Player.PlayerRef.CurrentTarget == null)
            {
                _enemyEngaged = false;
                _table.removeElement(_enemyHealthBar);
            }
        }

        private void ShowAndRemoveEnemyHealth()
        {
            if (entity.scene != null)
            {
                AddEnemyHealth();
                RemoveEnemyHealth();
                UpdateEnemyHealth();
                
            }
        }

        private void SetupScore()
        {
            var scoreString = "Score: " + _score;
            _scoreEntity = entity.scene.createEntity("score");
            _scoreEntity.addComponent(new Text(Graphics.instance.bitmapFont, scoreString, new Vector2(Screen.width / 2.4f, 1), Color.White))
                .setRenderLayer(ScreenSpaceRenderLayer);
           _scoreEntity.scale = new Vector2(2f);
        }

        private void UpdateScore()
        {
            _score = Player.PlayerRef.Score;
            _scoreEntity.getComponent<Text>().setText("Score: " + _score);
        }

        private void ShowTutorial()
        {
            _tutorial = entity.scene.createEntity("tutorial");
            _tutorial.addComponent(new Text(Graphics.instance.bitmapFont, "Press E to See...", new Vector2(20, Screen.height - 20f), Color.White)).setRenderLayer(ScreenSpaceRenderLayer);
            _tutorial.scale = new Vector2(2);
        }

        private void RemoveTutorial()
        {
            if (Player.PlayerRef.HasUsedGlasses && _tutorial!= null)
            {
                _tutorial.getComponent<Text>().setText("");
            }
        }

    }//end class
}//end namespace
