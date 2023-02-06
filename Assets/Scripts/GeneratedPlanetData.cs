using UnityEngine;

using Earthgen.planet;
using Earthgen.planet.terrain;
using Earthgen.planet.climate;

namespace Earthgen.unity
{
    public class GeneratedPlanetData : MonoBehaviour
    {
        public Planet planet = new();
        public Terrain_parameters terrainParameters;
        public Climate_parameters climateParameters;
    }
}
