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
        private float _totalLife;
        public HitController()
        {
        }

        void IUpdatable.update()
        {   
        }
        public void DoDamage(float inDamage)
        {
            entity.getComponent<Enemy>().HandleHits(inDamage);
            _totalLife -= inDamage;
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
        public float GetLife()
        {
            _totalLife = entity.getComponent<Enemy>().CurrentLife;
            return _totalLife;
        }
    }
}
