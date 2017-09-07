using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez;

namespace EyesHaveIt.Utilities
{
    public class HitController : Component, IUpdatable
    {
        float totalLife;
        public HitController()
        {
        }

        void IUpdatable.update()
        {   
        }
        public void doDamage(float inDamage)
        {
            entity.getComponent<Enemy>().handleHits(inDamage);
            totalLife -= inDamage;
            Debug.log("Doing Damage");
            /*
         
            Debug.log("Doing Damage");
            if (totalLife > 0)
            {
                
                totalLife -= inDamage;
            }
            if (totalLife == 0)
            {
                entity.detachFromScene();
                //entity.destroy();
               // entity.removeAllComponents();
            }*/
        }
        public float getLife()
        {
            totalLife = entity.getComponent<Enemy>().currentLife;
            return totalLife;
        }
    }
}
