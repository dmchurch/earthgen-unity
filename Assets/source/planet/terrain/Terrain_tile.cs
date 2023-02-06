using System;

namespace Earthgen.planet.terrain
{
    [Serializable]
    public struct Terrain_tile
    {
        public float elevation;
        public Terrain_water water;
        public Type type;

        public bool is_land => type.HasFlag(Type.Land);
        public bool is_water => type.HasFlag(Type.Water);
        public bool has_coast => type.HasFlag(Type.Coast);

        [Flags]
        public enum Type
        {
            Land = 1,
            Water = 2,
            Coast = 4,
        }
    }
}

namespace Earthgen
{
    using Earthgen.planet.terrain;

    public static partial class Statics
    {
        public static bool is_land (Terrain_tile t) => t.is_land;
        public static bool is_water (Terrain_tile t) => t.is_water;
        public static bool has_coast (Terrain_tile t) => t.has_coast;

        public static float elevation (Terrain_tile t) => t.elevation;
        public static float water_depth (Terrain_tile t) => t.water.depth;
    }
}
