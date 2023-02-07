using UnityEngine;

using Earthgen.planet;
using Earthgen.planet.terrain;
using Earthgen.planet.climate;
using Earthgen.planet.grid;

namespace Earthgen.unity
{
    public class GeneratedPlanetData : MonoBehaviour
    {
        public Planet planet = new();
        public Terrain_parameters terrainParameters;
        public Climate_parameters climateParameters;

        private void Awake()
        {
            Debug.Log($"Awake, setting grid size {terrainParameters.grid_size}");
            planet.set_grid_size(terrainParameters.grid_size);
        }
    }
}
