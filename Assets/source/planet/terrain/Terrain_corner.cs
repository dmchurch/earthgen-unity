using System;
using UnityEngine;

namespace Earthgen.planet.terrain
{
    [Serializable]
    public struct Terrain_corner
    {
        public static Terrain_corner Default => new() {
            type = Terrain.Type.land,
            elevation = 0,
            river_direction = -1,
            distance_to_sea = -1,
        };
        public void Reset()
        {
            elevation = 0;
            river_direction = -1;
            distance_to_sea = -1;
            type = Terrain.Type.land;
        }
        public float elevation;
        public int river_direction;
        public int distance_to_sea;
        public Terrain.Type type;

        public bool is_land() => type == Terrain.Type.land;
        public bool is_water() => type == Terrain.Type.water;
        public bool is_coast() => type == Terrain.Type.coast;
    }
}