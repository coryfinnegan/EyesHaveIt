using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;
using Nez.Tiled;
using Microsoft.Xna.Framework;

namespace EyesHaveIt.Utilities
{
    

    class EnemySpawnController : Component, IUpdatable
    {
        TiledMap tiledMap;
        TiledObject[] mapObjects;
        List<TiledObject> spawnLocations;
        TiledObjectGroup objectLayer;
        int spawnCounter = 0;
        Vector2 leftOffset = new Vector2(-Screen.width+10, 0);
        Vector2 rightOffset = new Vector2(Screen.width+10, 0);
        Vector2 topOffset = new Vector2(0, -40);
        Vector2 bottomOffset = new Vector2(0, 60);

        public EnemySpawnController(TiledMap tiledMap)
        {
            this.tiledMap = tiledMap;
            this.objectLayer = tiledMap.getObjectGroup("objects");

        }
        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            mapObjects = objectLayer.objects;
            spawnLocations = objectLayer.objectsWithName("enemyWave");
            

        }
        void spawnEnemies()
        {
            foreach (var position in spawnLocations){
                if (checkForPlayerPosition(position))
                {
                    //var newSpawnLocation = new Vector2(position.x, position.y);
                    //spawnEnemyPositions(newSpawnLocation, spawnCounter);
                    spawnCounter++;
                    //spawnPunk(position, spawnCounter);
                } 
            }
        }
        void spawnEnemyPositions()
        {
            var position = Player.playerRef.transform.position;
            //first wave
            if (spawnCounter == 0 && position.X >= spawnLocations[0].x)
            {
                Debug.log("Spawn 1");
                spawnPunk(position + topOffset + rightOffset);
                var task = Core.schedule(2f, false, timer => spawnPunk(position + topOffset + rightOffset));
                spawnCounter++;
                //spawnPunk(position + bottomOffset + rightOffset);
            }
            //second wave
            if (spawnCounter == 1 && position.X >= spawnLocations[1].x)
            {
                spawnCounter++;
                spawnPunk(position + leftOffset + topOffset);
                spawnAgent(position + rightOffset);
                Core.schedule(2f, false, timer => spawnPunk(position + topOffset + rightOffset));
                Core.schedule(2f, false, timer => spawnAgent(position + leftOffset));
            }
            //third wave
            if (spawnCounter == 2 && position.X >= spawnLocations[2].x)
            {
                spawnCounter++;
                spawnPunk(position + leftOffset + topOffset);
                spawnPunk(position + topOffset + rightOffset);
                spawnAgent(position + rightOffset);
                spawnAgent(position + leftOffset);
            }
            if (spawnCounter == 0 && position.X == spawnLocations[0].x)
            {
                spawnCounter++;
            }
        }
        void spawnPunk(Vector2 position)
        {
            
            var punk = entity.scene.createEntity("punk");
            punk.addComponent(new Actors.Punk("Punk", 4, tiledMap, false, 4));
            
            punk.scale = new Vector2(2);
            punk.transform.position = position;
        }
        void spawnAgent(Vector2 position)
        {
            var agent = entity.scene.createEntity("agent");
            agent.addComponent(new Enemy("EnemyAgent", 8, tiledMap, true, 10));
            
            agent.scale = new Vector2(2);
            agent.transform.position = position;
        }
        bool checkForPlayerPosition(TiledObject position)
        {
            if (Player.playerRef.transform.position.X == position.x)
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
            
            spawnEnemyPositions();
        }
    }
}
