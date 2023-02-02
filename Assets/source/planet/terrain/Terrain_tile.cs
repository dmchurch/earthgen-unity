using System;

namespace Earthgen.planet.terrain
{
    [Serializable]
    public struct Terrain_tile
    {
        public static Terrain_tile Default => new()
        {
            type = Terrain.Type.land,
        };
        public float elevation;
        public Terrain_water water;
        public Terrain.Type type;

        public bool is_land() => type.HasFlag(Terrain.Type.land);
        public bool is_water() => type.HasFlag(Terrain.Type.water);
        public bool has_coast() => type.HasFlag(Terrain.Type.coast);

        public float water_depth() => water.depth;
    }
}