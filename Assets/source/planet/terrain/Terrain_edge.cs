using System;

namespace Earthgen.planet.terrain
{
    [Serializable]
    public struct Terrain_edge
    {
        public static Terrain_edge Default => new()
        {
            type = Terrain.Type.land,
        };
        public Terrain.Type type;

        public bool is_land() => type == Terrain.Type.land;
        public bool is_water() => type == Terrain.Type.water;
        public bool is_coast() => type == Terrain.Type.coast;
    }
}