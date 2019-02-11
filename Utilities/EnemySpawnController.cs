using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EyesHaveIt.Enums;
using Nez;
using Nez.Tiled;
using Microsoft.Xna.Framework;

namespace EyesHaveIt.Utilities
{
    internal class EnemySpawnController : Component, IUpdatable
    {
        private TiledMap _tiledMap;
        private TiledObject[] _mapObjects;
        private List<TiledObject> _spawnLocations;
        private TiledObjectGroup _objectLayer;
        private int _spawnCounter = 0;
        private Vector2 _leftOffset = new Vector2(-Screen.width+10, 0);
        private Vector2 _rightOffset = new Vector2(Screen.width+10, 0);
        private Vector2 _topOffset = new Vector2(0, -40);
        private Vector2 _bottomOffset = new Vector2(0, 60);

        public EnemySpawnController(TiledMap tiledMap)
        {
            this._tiledMap = tiledMap;
            this._objectLayer = tiledMap.getObjectGroup("objects");

        }
        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            _mapObjects = _objectLayer.objects;
            _spawnLocations = _objectLayer.objectsWithName("enemyWave");
            

        }

        private void SpawnEnemies()
        {
            foreach (var position in _spawnLocations){
                if (CheckForPlayerPosition(position))
                {
                    //var newSpawnLocation = new Vector2(position.x, position.y);
                    //spawnEnemyPositions(newSpawnLocation, spawnCounter);
                    _spawnCounter++;
                    //spawnPunk(position, spawnCounter);
                } 
            }
        }

        private void SpawnEnemyPositions()
        {
            var position = Player.PlayerRef.transform.position;
            //first wave
            if (_spawnCounter == 0 && position.X >= _spawnLocations[0].x)
            {
                Debug.log("Spawn 1");
                SpawnPunk(position + _topOffset + _rightOffset);
                var task = Core.schedule(2f, false, timer => SpawnPunk(position + _topOffset + _rightOffset));
                _spawnCounter++;
                //spawnPunk(position + bottomOffset + rightOffset);
            }
            //second wave
            if (_spawnCounter == 1 && position.X >= _spawnLocations[1].x)
            {
                _spawnCounter++;
                SpawnPunk(position + _leftOffset + _topOffset);
                SpawnAgent(position + _rightOffset);
                Core.schedule(2f, false, timer => SpawnPunk(position + _topOffset + _rightOffset));
                Core.schedule(2f, false, timer => SpawnAgent(position + _leftOffset));
            }
            //third wave
            if (_spawnCounter == 2 && position.X >= _spawnLocations[2].x)
            {
                _spawnCounter++;
                SpawnPunk(position + _leftOffset + _topOffset);
                SpawnPunk(position + _topOffset + _rightOffset);
                SpawnAgent(position + _rightOffset);
                SpawnAgent(position + _leftOffset);
            }
            if (_spawnCounter == 0 && position.X == _spawnLocations[0].x)
            {
                _spawnCounter++;
            }
        }

        private void SpawnPunk(Vector2 position)
        {
            
            var punk = entity.scene.createEntity("punk");
            punk.addComponent(new Actors.Punk("Punk", 4, _tiledMap, false, 4));
            
            punk.scale = new Vector2(2);
            punk.transform.position = position;
        }

        private void SpawnAgent(Vector2 position)
        {
            var agent = entity.scene.createEntity("agent");
            agent.addComponent(new Enemy("EnemyAgent", 8, _tiledMap, true, 10));
            
            agent.scale = new Vector2(2);
            agent.transform.position = position;
        }

        private bool CheckForPlayerPosition(TiledObject position)
        {
            if (Player.PlayerRef.transform.position.X == position.x)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        void IUpdatable.update()
        {
            
            SpawnEnemyPositions();
        }
    }
}
