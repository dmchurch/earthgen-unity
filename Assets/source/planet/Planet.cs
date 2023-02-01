using System.Collections;
using System.Collections.Generic;

using Earthgen.planet.grid;
//using Earthgen.planet.climate;
using Earthgen.planet.terrain;

namespace Earthgen.planet
{
    [UnityEngine.CreateAssetMenu]
    public class Planet : UnityEngine.ScriptableObject
    {
        public void OnEnable()
        {
            if (!grid) {
                grid = Grid.size_n_grid(0);
                terrain = CreateInstance<Terrain>();
                //climate = CreateInstance<Climate>();
            }
        }

        public Grid grid;
        public Terrain terrain;
        //public Climate climate;

        [UnityEngine.ContextMenu("Clear Planet")]
        public void Clear()
        {
            this.set_grid_size(0);
            this.clear_terrain();
            //this.clear_climate();
        }


    };
}
